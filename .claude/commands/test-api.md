# /test-api

Run a quick smoke test against the API.

## Usage
```
/test-api           # Tests local (https://localhost:7001)
/test-api azure     # Tests deployed Azure URL from context
```

## What This Command Does

1. Determine base URL from argument or `context/project-context.md`
2. Run health check first — if it fails, stop and show logs
3. Run CRUD tests for the active phase endpoints
4. Print pass ✅ / fail ❌ per endpoint

## Test Script Template

```bash
BASE="https://localhost:7001"

echo "=== Smoke Test: $BASE ==="

# Health
CODE=$(curl -sk -o /dev/null -w "%{http_code}" $BASE/health)
[[ $CODE == "200" ]] && echo "✅ GET /health" || echo "❌ GET /health — $CODE"

# GET all
CODE=$(curl -sk -o /dev/null -w "%{http_code}" $BASE/api/tasks)
[[ $CODE == "200" ]] && echo "✅ GET /api/tasks" || echo "❌ GET /api/tasks — $CODE"

# POST create
BODY='{"title":"Test task","priority":2}'
RESP=$(curl -sk -X POST $BASE/api/tasks -H "Content-Type: application/json" -d "$BODY")
echo $RESP | grep -q '"id"' && echo "✅ POST /api/tasks" || echo "❌ POST /api/tasks — $RESP"

# GET by ID
ID=$(echo $RESP | python3 -c "import sys,json; print(json.load(sys.stdin).get('id','?'))")
CODE=$(curl -sk -o /dev/null -w "%{http_code}" $BASE/api/tasks/$ID)
[[ $CODE == "200" ]] && echo "✅ GET /api/tasks/$ID" || echo "❌ GET /api/tasks/$ID — $CODE"

# DELETE
CODE=$(curl -sk -o /dev/null -w "%{http_code}" -X DELETE $BASE/api/tasks/$ID)
[[ $CODE == "204" ]] && echo "✅ DELETE /api/tasks/$ID" || echo "❌ DELETE /api/tasks/$ID — $CODE"

echo "=== Done ==="
```

## Notes
- Use `-k` flag for local HTTPS (self-signed cert is fine)
- Tests are adapted per phase — Phase 2 tests file upload, Phase 4 tests messaging, etc.
