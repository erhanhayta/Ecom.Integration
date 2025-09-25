import { api } from "./client";

export async function getCategories(shopId, { parentId = null, q = "" } = {}) {
  const qs = new URLSearchParams();
  if (parentId != null) qs.set("parentId", parentId);
  if (q) qs.set("q", q);
  return api(`/admin/MarketplaceCatalog/${shopId}/categories?${qs.toString()}`);
}

export async function getCategoryAttributes(shopId, categoryId) {
  return api(`/admin/MarketplaceCatalog/${shopId}/categories/${categoryId}/attributes`);
}
