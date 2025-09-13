const BASE = import.meta.env.VITE_API_BASE || "";

export function getToken(){ return localStorage.getItem("ecom_token") || ""; }
export function setToken(t){ if(!t) localStorage.removeItem("ecom_token"); else localStorage.setItem("ecom_token", t); }

export async function api(path, { method="GET", headers={}, body } = {}){
  const token = getToken();
  const res = await fetch(BASE + path, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body,
  });
  if(!res.ok){
    const text = await res.text().catch(()=> "");
    throw new Error(text || `HTTP ${res.status}`);
  }
  const ct = res.headers.get("content-type") || "";
  return ct.includes("application/json") ? res.json() : res.text();
}
