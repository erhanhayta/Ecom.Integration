import { api } from "./client";

// Mağazalar
export async function listShops() {
  return api("/admin/MarketplaceShops");
}

// Kategoriler (drill-down: parentId vererek seviye seviye çağır)
export async function getTyCategories(shopId, { parentId = null, q = "" } = {}) {
  const qs = new URLSearchParams();
  if (parentId != null) qs.set("parentId", parentId);
  if (q) qs.set("q", q);
  const suffix = qs.toString() ? `?${qs.toString()}` : "";
  return api(`/admin/MarketplaceCatalog/${shopId}/categories${suffix}`);
}

// Kategori özellikleri
export async function getTyCategoryAttributes(shopId, categoryId) {
  return api(`/admin/MarketplaceCatalog/${shopId}/categories/${categoryId}/attributes`);
}

// Kaydedilmiş config’i getir/kaydet
export async function getTyConfig(shopId, productId) {
  return api(`/admin/ty/${shopId}/products/${productId}/config`);
}
export async function saveTyConfig(shopId, productId, payload) {
  return api(`/admin/ty/${shopId}/products/${productId}/config`, {
    method: "POST",
    body: payload,
  });
}

// Bizdeki varyantlardan TY varianter eşlemesi üret
export async function mapTyVariants(shopId, productId) {
  return api(`/admin/ty/${shopId}/products/${productId}/map-variants`, { method: "POST" });
}

// Breadcrumb (seviye seviye geri doldurma) – backend’e eklemeden
// public ağaç yoksa bu fonksiyonu UI tarafında adım adım parentId ile çözümleyeceğiz.
// Şimdilik boş bırakıyoruz; UI tarafında parentId zinciri ile dolduracağız.
