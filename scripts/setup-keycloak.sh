#!/usr/bin/env bash
# ProcureHub — Keycloak automated setup script
#
# Usage:
#   bash scripts/setup-keycloak.sh
#
# Override defaults via environment variables:
#   KC_URL=http://localhost:9090 KC_ADMIN=admin KC_ADMIN_PASSWORD=admin bash scripts/setup-keycloak.sh
#
# After this script completes:
#   mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD $DB_NAME < scripts/keycloak-id-updates.sql

set -euo pipefail

# ── Configuration (override via env) ────────────────────────────────────────
KC_URL="${KC_URL:-http://localhost:9090}"
KC_ADMIN="${KC_ADMIN:-admin}"
KC_ADMIN_PASSWORD="${KC_ADMIN_PASSWORD:-admin}"
KC_REALM="procurehub"
DB_HOST="${DB_HOST:-127.0.0.1}"
DB_PORT="${DB_PORT:-3307}"
DB_NAME="${DB_NAME:-procurehub}"
DB_USER="${DB_USER:-root}"
DB_PASSWORD="${DB_PASSWORD:-rootsecret}"
TEMP_PASSWORD="${TEMP_PASSWORD:-ProcureHub@2024!}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REALM_JSON="${SCRIPT_DIR}/../keycloak/realm-export.json"
SQL_OUTPUT="${SCRIPT_DIR}/keycloak-id-updates.sql"

# ── Colors ───────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

info()    { echo -e "${GREEN}✓${NC} $*"; }
warn()    { echo -e "${YELLOW}⚠${NC}  $*"; }
error()   { echo -e "${RED}✗${NC} $*" >&2; }
die()     { error "$*"; exit 1; }

# ── Wait for Keycloak ────────────────────────────────────────────────────────
wait_for_keycloak() {
    echo "Waiting for Keycloak at ${KC_URL} ..."
    local attempts=0
    local max=40  # 40 × 3s = 120s timeout
    until curl -sf "${KC_URL}/health/ready" > /dev/null 2>&1; do
        attempts=$((attempts + 1))
        if [ "$attempts" -ge "$max" ]; then
            die "Keycloak did not become ready within 120 seconds. Is it running?"
        fi
        echo -n "."
        sleep 3
    done
    echo ""
    info "Keycloak is ready"
}

# ── Obtain Admin Token ───────────────────────────────────────────────────────
ADMIN_TOKEN=""
get_admin_token() {
    local response
    response=$(curl -sf -X POST \
        "${KC_URL}/realms/master/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "grant_type=password" \
        -d "client_id=admin-cli" \
        -d "username=${KC_ADMIN}" \
        -d "password=${KC_ADMIN_PASSWORD}") || die "Failed to authenticate with Keycloak. Check KC_ADMIN and KC_ADMIN_PASSWORD."

    ADMIN_TOKEN=$(echo "$response" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)
    [ -n "$ADMIN_TOKEN" ] || die "Could not parse access_token from Keycloak response."
    info "Admin token obtained"
}

# ── Import Realm ─────────────────────────────────────────────────────────────
import_realm() {
    [ -f "$REALM_JSON" ] || die "Realm export file not found: ${REALM_JSON}"

    local status
    status=$(curl -s -o /dev/null -w "%{http_code}" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}" \
        "${KC_URL}/admin/realms/${KC_REALM}")

    if [ "$status" = "200" ]; then
        warn "Realm '${KC_REALM}' already exists — skipping import"
        return
    fi

    curl -sf -X POST \
        "${KC_URL}/admin/realms" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}" \
        -H "Content-Type: application/json" \
        -d "@${REALM_JSON}" || die "Failed to import realm from ${REALM_JSON}"

    info "Realm '${KC_REALM}' imported"
}

# ── Create User + Assign Role ────────────────────────────────────────────────
# Returns the Keycloak user UUID via stdout.
create_user() {
    local email="$1"
    local first="$2"
    local last="$3"
    local role="$4"

    # Check if user already exists
    local existing
    existing=$(curl -sf \
        "${KC_URL}/admin/realms/${KC_REALM}/users?email=${email}&exact=true" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

    local user_id
    if [ -n "$existing" ]; then
        warn "User ${email} already exists — skipping creation"
        user_id="$existing"
    else
        # Create user
        curl -sf -X POST \
            "${KC_URL}/admin/realms/${KC_REALM}/users" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" \
            -H "Content-Type: application/json" \
            -d "{
                \"username\": \"${email}\",
                \"email\": \"${email}\",
                \"firstName\": \"${first}\",
                \"lastName\": \"${last}\",
                \"enabled\": true,
                \"emailVerified\": true,
                \"credentials\": [{
                    \"type\": \"password\",
                    \"value\": \"${TEMP_PASSWORD}\",
                    \"temporary\": true
                }]
            }" || die "Failed to create user: ${email}"

        # Fetch the newly created user's ID
        user_id=$(curl -sf \
            "${KC_URL}/admin/realms/${KC_REALM}/users?email=${email}&exact=true" \
            -H "Authorization: Bearer ${ADMIN_TOKEN}" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

        [ -n "$user_id" ] || die "Could not retrieve UUID for newly created user: ${email}"
    fi

    # Get role representation
    local role_json
    role_json=$(curl -sf \
        "${KC_URL}/admin/realms/${KC_REALM}/roles/${role}" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}") || die "Role '${role}' not found in realm '${KC_REALM}'"

    # Assign realm role
    curl -sf -X POST \
        "${KC_URL}/admin/realms/${KC_REALM}/users/${user_id}/role-mappings/realm" \
        -H "Authorization: Bearer ${ADMIN_TOKEN}" \
        -H "Content-Type: application/json" \
        -d "[${role_json}]" || die "Failed to assign role '${role}' to user ${email}"

    info "  ${email} → ${role} (${user_id})"
    echo "$user_id"
}

# ── Generate SQL ─────────────────────────────────────────────────────────────
generate_sql() {
    local -n map=$1  # associative array: email → uuid
    {
        echo "-- Auto-generated by scripts/setup-keycloak.sh"
        echo "-- Run AFTER executing the setup script."
        echo "-- Updates keycloak_id for all internal (non-vendor) users."
        echo ""
        echo "USE \`${DB_NAME}\`;"
        echo ""
        for email in "${!map[@]}"; do
            echo "UPDATE users SET keycloak_id = '${map[$email]}' WHERE email = '${email}';"
        done
        echo ""
        echo "-- Verify (expected: 10 rows updated)"
        echo "SELECT email, keycloak_id FROM users WHERE keycloak_id IS NOT NULL AND keycloak_id != '' ORDER BY email;"
    } > "$SQL_OUTPUT"
    info "SQL written to ${SQL_OUTPUT}"
}

# ── Main ─────────────────────────────────────────────────────────────────────
main() {
    echo ""
    echo "==========================================="
    echo "  ProcureHub — Keycloak Setup Script"
    echo "  Target: ${KC_URL}"
    echo "  Realm:  ${KC_REALM}"
    echo "==========================================="
    echo ""

    wait_for_keycloak
    get_admin_token
    import_realm

    echo ""
    echo "Creating internal users ..."

    # email → uuid map for SQL generation
    declare -A user_map

    # Internal users — must match scripts/seed.sql exactly
    user_map["admin@surya-abadi.co.id"]=$(create_user \
        "admin@surya-abadi.co.id" "System" "Administrator" "super_admin")

    user_map["agus.prasetyo@surya-abadi.co.id"]=$(create_user \
        "agus.prasetyo@surya-abadi.co.id" "Agus" "Prasetyo" "purchasing")

    user_map["dewi.kusuma@surya-abadi.co.id"]=$(create_user \
        "dewi.kusuma@surya-abadi.co.id" "Dewi" "Kusuma" "purchasing")

    user_map["budi.santoso@surya-abadi.co.id"]=$(create_user \
        "budi.santoso@surya-abadi.co.id" "Budi" "Santoso" "requester")

    user_map["sari.wijaya@surya-abadi.co.id"]=$(create_user \
        "sari.wijaya@surya-abadi.co.id" "Sari" "Wijaya" "requester")

    user_map["bambang.sutrisno@surya-abadi.co.id"]=$(create_user \
        "bambang.sutrisno@surya-abadi.co.id" "Bambang" "Sutrisno" "approver")

    user_map["hendra.gunawan@surya-abadi.co.id"]=$(create_user \
        "hendra.gunawan@surya-abadi.co.id" "Hendra" "Gunawan" "approver")

    user_map["director@surya-abadi.co.id"]=$(create_user \
        "director@surya-abadi.co.id" "Hadi" "Purnomo" "approver")

    user_map["rina.marlina@surya-abadi.co.id"]=$(create_user \
        "rina.marlina@surya-abadi.co.id" "Rina" "Marlina" "finance")

    user_map["cfo@surya-abadi.co.id"]=$(create_user \
        "cfo@surya-abadi.co.id" "Santoso" "Wibowo" "management")

    echo ""
    generate_sql user_map

    echo ""
    echo "==========================================="
    info "Setup complete — ${#user_map[@]} users processed"
    echo ""
    echo "Next step:"
    echo "  mysql -h ${DB_HOST} -P ${DB_PORT} -u ${DB_USER} -p${DB_PASSWORD} ${DB_NAME} < ${SQL_OUTPUT}"
    echo ""
    echo "All users have a temporary password: ${TEMP_PASSWORD}"
    echo "Users must change it on first login."
    echo "==========================================="
    echo ""
}

main "$@"
