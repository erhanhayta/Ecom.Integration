// src/api/client.js
const BASE = import.meta.env.VITE_API_BASE || "";

export function getToken() {
  return localStorage.getItem("ecom_token") || "";
}
export function setToken(t) {
  if (!t) localStorage.removeItem("ecom_token");
  else localStorage.setItem("ecom_token", t);
}

export async function api(path, opts = {}) {
  const {
    method = "GET",
    headers: extraHeaders = {},
    body: rawBody,
  } = opts;

  const token = getToken();

  // FormData mı?
  const isFormData =
    typeof FormData !== "undefined" && rawBody instanceof FormData;

  // Header'ları hazırla (FormData ise Content-Type koyma!)
  const headers = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...extraHeaders,
  };
  if (!isFormData) {
    // JSON ise Content-Type ayarla (kullanıcı özel bir Content-Type vermediyse)
    if (!headers["Content-Type"]) headers["Content-Type"] = "application/json";
  }

  // Body'yi hazırla (JSON ise stringify et)
  let body = rawBody;
  if (!isFormData && rawBody && headers["Content-Type"] === "application/json" && typeof rawBody !== "string") {
    body = JSON.stringify(rawBody);
  }

  const res = await fetch(BASE + path, {
    method,
    headers,
    body,
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `HTTP ${res.status}`);
  }

  const ct = res.headers.get("content-type") || "";
  return ct.includes("application/json") ? res.json() : res.text();
}
