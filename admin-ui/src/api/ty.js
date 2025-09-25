import { api } from "./client";

export async function listShops() {
  return api("/admin/MarketplaceShops");
}

export async function getTyCategories(shopId, { parentId = null, q = "" } = {}) {
  const qs = new URLSearchParams();
  if (parentId != null) qs.set("parentId", parentId);
  if (q) qs.set("q", q);
  const suffix = qs.toString() ? `?${qs.toString()}` : "";
  return api(`/admin/MarketplaceCatalog/${shopId}/categories${suffix}`);
}

export async function getTyCategoryAttributes(shopId, categoryId) {
  return api(`/admin/MarketplaceCatalog/${shopId}/categories/${categoryId}/attributes`);
}

export async function saveTyConfig(shopId, productId, payload) {
  return api(`/admin/ty/${shopId}/products/${productId}/config`, {
    method: "POST",
    body: payload,
  });
}

export async function mapTyVariants(shopId, productId) {
  return api(`/admin/ty/${shopId}/products/${productId}/map-variants`, {
    method: "POST",
  });
}
