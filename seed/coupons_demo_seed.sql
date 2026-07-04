-- =============================================================================
-- Rahal — Coupons demo seed (idempotent)
-- =============================================================================
-- Seeds the coupon catalog for the front-end "Coupons" vendor catalog + wallet.
--
-- Context:
--   * The dev DB ships 4 approved vendors (gamification."VendorProfiles") but 0
--     coupons. This adds coupons for each, plus a few pre-claimed wallet rows for
--     the demo explorer so the wallet's status filters have content on first run.
--   * Coupon.VendorId is set to the vendor's *user id* (d0a…). VendorProfiles' PK is
--     UserId, so GET /VendorProfile/{id} (the catalog's name lookup) keys on the user
--     id — and so does vendor-side redeem (POST /UserCoupon/redeem matches
--     Coupon.VendorId == the vendor's login user id). Both reconcile on d0a….
--   * Enum columns (DiscountType, Status) are stored as their .NET names.
--   * Demo explorer = layla.demo@rahal.test (user id e0a…0001), which already has
--     ~1000 available XP, so live claims of the un-pre-claimed coupons work.
--
-- Re-runnable: deletes its own fixed-id rows (and any wallet rows referencing the
-- seeded coupons) before re-inserting.
-- Apply:  docker exec -i rahal-backend-postgres-container-1 \
--           psql -U postgres -d Rahal < seed/coupons_demo_seed.sql
-- =============================================================================

BEGIN;

-- Vendor user ids (Coupon.VendorId → these; VendorProfiles PK = UserId).
--   d0a…0001 Sahara Bean Café   | d0a…0002 Nile Pearl Cruises
--   d0a…0003 Khan Relics Bazaar | d0a…0004 Red Sea Divers Co.

-- Demo explorer (wallet owner).
--   e0a…0001 layla.demo@rahal.test

-- --- Clean up prior runs (wallet rows first: FK UserCoupons.CouponId → Coupons) ---
DELETE FROM rewards."UserCoupons"
WHERE "CouponId" IN (
  'c0c00000-0000-4000-8000-000000000001','c0c00000-0000-4000-8000-000000000002',
  'c0c00000-0000-4000-8000-000000000003','c0c00000-0000-4000-8000-000000000004',
  'c0c00000-0000-4000-8000-000000000005','c0c00000-0000-4000-8000-000000000006',
  'c0c00000-0000-4000-8000-000000000007','c0c00000-0000-4000-8000-000000000008',
  'c0c00000-0000-4000-8000-000000000009'
);

DELETE FROM rewards."Coupons"
WHERE "Id" IN (
  'c0c00000-0000-4000-8000-000000000001','c0c00000-0000-4000-8000-000000000002',
  'c0c00000-0000-4000-8000-000000000003','c0c00000-0000-4000-8000-000000000004',
  'c0c00000-0000-4000-8000-000000000005','c0c00000-0000-4000-8000-000000000006',
  'c0c00000-0000-4000-8000-000000000007','c0c00000-0000-4000-8000-000000000008',
  'c0c00000-0000-4000-8000-000000000009'
);

-- --- Coupons ----------------------------------------------------------------
INSERT INTO rewards."Coupons"
  ("Id","VendorId","Title","Description","XpCost","DiscountType","DiscountValue",
   "MaxDiscountValue","MinimumCharge","MaxClaims","CurrentClaims","ExpiresAt",
   "IsActive","CreatedAt","IsDeleted")
VALUES
  -- Sahara Bean Café
  ('c0c00000-0000-4000-8000-000000000001','d0a00000-0000-4000-8000-000000000001',
   '20% Off Your Morning Brew','Any espresso-based drink before noon.',
   20,'Percentage',20,40,50,100,0, now() + interval '30 days', true, now(), false),
  ('c0c00000-0000-4000-8000-000000000002','d0a00000-0000-4000-8000-000000000001',
   'EGP 50 Off Weekend Brunch','Valid on the à la carte brunch menu.',
   40,'FixedAmount',50,NULL,200,50,0, now() + interval '30 days', true, now(), false),
  ('c0c00000-0000-4000-8000-000000000003','d0a00000-0000-4000-8000-000000000001',
   'Free Cold Brew (Sold Out)','A limited seasonal pour — fully claimed.',
   25,'FixedAmount',45,NULL,45,30,30, now() + interval '30 days', true, now(), false),

  -- Nile Pearl Cruises
  ('c0c00000-0000-4000-8000-000000000004','d0a00000-0000-4000-8000-000000000002',
   '15% Off Sunset Cruise','Golden-hour sailing along the Aswan Corniche.',
   60,'Percentage',15,120,300,40,0, now() + interval '30 days', true, now(), false),
  ('c0c00000-0000-4000-8000-000000000005','d0a00000-0000-4000-8000-000000000002',
   'EGP 100 Off Dinner Cruise','Four-course dinner aboard the Nile Pearl.',
   80,'FixedAmount',100,NULL,500,25,0, now() + interval '30 days', true, now(), false),

  -- Khan Relics Bazaar
  ('c0c00000-0000-4000-8000-000000000006','d0a00000-0000-4000-8000-000000000003',
   '10% Off Any Relic','Hand-picked brass, glass and textile finds.',
   30,'Percentage',10,200,100,200,0, now() + interval '30 days', true, now(), false),
  ('c0c00000-0000-4000-8000-000000000007','d0a00000-0000-4000-8000-000000000003',
   '25% Off Lanterns (Expired)','A past Ramadan promotion, kept for the archive.',
   35,'Percentage',25,150,150,60,4, now() - interval '5 days', true, now(), false),

  -- Red Sea Divers Co.
  ('c0c00000-0000-4000-8000-000000000008','d0a00000-0000-4000-8000-000000000004',
   '20% Off Intro Dive','A guided first dive off the Dahab reef.',
   50,'Percentage',20,300,400,30,0, now() + interval '30 days', true, now(), false),
  ('c0c00000-0000-4000-8000-000000000009','d0a00000-0000-4000-8000-000000000004',
   'EGP 150 Off Gear Rental','Full-day mask, fins and BCD rental.',
   45,'FixedAmount',150,NULL,300,40,0, now() + interval '30 days', true, now(), false);

-- --- Demo wallet (layla.demo) — mixed statuses for the filter tabs ----------
-- ExplorerId = user id (e0a…0001). Codes mimic the CPN-{guid} format.
-- Leaves c0c…0001, …0005, …0009 un-claimed so a live claim demo still works.
INSERT INTO rewards."UserCoupons"
  ("Id","ExplorerId","CouponId","Code","IsRedeemed","Status",
   "ClaimedAt","RedeemedAt","ExpiresAt","CreatedAt","IsDeleted")
VALUES
  -- Ready (backend "Claimed") — the user's "pending / not yet used at store"
  ('a0c00000-0000-4000-8000-000000000001','e0a00000-0000-4000-8000-000000000001',
   'c0c00000-0000-4000-8000-000000000002','CPN-DEMO-READY-0000000000000001',
   false,'Claimed', now() - interval '2 days', NULL, now() + interval '28 days', now(), false),
  ('a0c00000-0000-4000-8000-000000000004','e0a00000-0000-4000-8000-000000000001',
   'c0c00000-0000-4000-8000-000000000008','CPN-DEMO-READY-0000000000000002',
   false,'Claimed', now() - interval '1 days', NULL, now() + interval '29 days', now(), false),
  -- Redeemed (used/scanned at the store)
  ('a0c00000-0000-4000-8000-000000000002','e0a00000-0000-4000-8000-000000000001',
   'c0c00000-0000-4000-8000-000000000006','CPN-DEMO-USED-00000000000000001',
   true,'Redeemed', now() - interval '10 days', now() - interval '3 days', now() + interval '20 days', now(), false),
  -- Expired
  ('a0c00000-0000-4000-8000-000000000003','e0a00000-0000-4000-8000-000000000001',
   'c0c00000-0000-4000-8000-000000000004','CPN-DEMO-EXPIRED-000000000000001',
   false,'Expired', now() - interval '40 days', NULL, now() - interval '2 days', now(), false);

COMMIT;

-- --- Verify -----------------------------------------------------------------
SELECT c."Title", v."DisplayName", c."DiscountType", c."DiscountValue", c."XpCost",
       c."CurrentClaims" || '/' || c."MaxClaims" AS claims, c."ExpiresAt"::date
FROM rewards."Coupons" c
JOIN gamification."VendorProfiles" v ON v."UserId" = c."VendorId"
ORDER BY v."DisplayName", c."Title";

SELECT uc."Code", uc."Status", uc."IsRedeemed", uc."ExpiresAt"::date
FROM rewards."UserCoupons" uc
WHERE uc."ExplorerId" = 'e0a00000-0000-4000-8000-000000000001'
ORDER BY uc."Status";
