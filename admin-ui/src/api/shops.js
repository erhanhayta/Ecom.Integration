import { api } from "./client";

/** Tüm mağazaları getirir */
export async function getShops() {
  return api("/admin/MarketplaceShops");
}

/** İlk aktif Trendyol mağazasını döndürür */
export async function getDefaultActiveTrendyolShop() {
  const list = await getShops();
  const trendyols = (list || []).filter(x => x.firm === 1 && x.isActive);
  return trendyols.length ? trendyols[0] : null;
}
