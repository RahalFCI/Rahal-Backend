-- =============================================================================
-- Rahal — Vendor WorkingHours repair (idempotent)
-- =============================================================================
-- Bug: gamification."VendorProfiles"."WorkingHours" was seeded with free-text
-- (e.g. "Daily 8:00-23:00"). The EF value converter reads the column as
-- JsonSerializer.Deserialize<Dictionary<DayOfWeek,string>>(...), so any non-JSON
-- value makes GET /VendorProfile/{id} throw a System.Text.Json.JsonException
-- ("'D' is an invalid start of a value") and 500. The Explorer coupon catalog
-- then can't resolve vendor names and shows the "Vendor" fallback for every group.
--
-- Fix: store a valid JSON object keyed by DayOfWeek name (the format documented on
-- users."VendorProfiles"."WorkingHours": {"Monday":"09:00-17:00", ...}), which the
-- converter round-trips cleanly. Keyed by UserId so re-running is safe.
--
-- Apply:  docker exec -i rahal-backend-postgres-container-1 \
--           psql -U postgres -d Rahal < seed/vendor_workinghours_fix.sql
-- =============================================================================

BEGIN;

-- Sahara Bean Café — daily 08:00–23:00
UPDATE gamification."VendorProfiles"
SET "WorkingHours" = '{"Sunday":"08:00-23:00","Monday":"08:00-23:00","Tuesday":"08:00-23:00","Wednesday":"08:00-23:00","Thursday":"08:00-23:00","Friday":"08:00-23:00","Saturday":"08:00-23:00"}'
WHERE "UserId" = 'd0a00000-0000-4000-8000-000000000001';

-- Nile Pearl Cruises — daily 09:00–20:00
UPDATE gamification."VendorProfiles"
SET "WorkingHours" = '{"Sunday":"09:00-20:00","Monday":"09:00-20:00","Tuesday":"09:00-20:00","Wednesday":"09:00-20:00","Thursday":"09:00-20:00","Friday":"09:00-20:00","Saturday":"09:00-20:00"}'
WHERE "UserId" = 'd0a00000-0000-4000-8000-000000000002';

-- Khan Relics Bazaar — Sat–Thu 10:00–22:00 (closed Friday)
UPDATE gamification."VendorProfiles"
SET "WorkingHours" = '{"Saturday":"10:00-22:00","Sunday":"10:00-22:00","Monday":"10:00-22:00","Tuesday":"10:00-22:00","Wednesday":"10:00-22:00","Thursday":"10:00-22:00"}'
WHERE "UserId" = 'd0a00000-0000-4000-8000-000000000003';

-- Red Sea Divers Co. — daily 07:00–18:00
UPDATE gamification."VendorProfiles"
SET "WorkingHours" = '{"Sunday":"07:00-18:00","Monday":"07:00-18:00","Tuesday":"07:00-18:00","Wednesday":"07:00-18:00","Thursday":"07:00-18:00","Friday":"07:00-18:00","Saturday":"07:00-18:00"}'
WHERE "UserId" = 'd0a00000-0000-4000-8000-000000000004';

-- Luxor Balloon Rides — daily 05:00–09:00
UPDATE gamification."VendorProfiles"
SET "WorkingHours" = '{"Sunday":"05:00-09:00","Monday":"05:00-09:00","Tuesday":"05:00-09:00","Wednesday":"05:00-09:00","Thursday":"05:00-09:00","Friday":"05:00-09:00","Saturday":"05:00-09:00"}'
WHERE "UserId" = 'd0a00000-0000-4000-8000-000000000005';

COMMIT;

-- Verify: all rows should now hold parseable JSON.
SELECT "DisplayName", "WorkingHours" FROM gamification."VendorProfiles" ORDER BY "DisplayName";
