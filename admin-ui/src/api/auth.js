import { api, setToken } from "./client";

export async function login(username, password){
  const res = await api("/auth/login", {
    method: "POST",
    body: JSON.stringify({ username, password }),
  });
  const token = res?.accessToken ?? res?.token ?? res?.jwt ?? res?.access_token ?? res?.value;
  if(!token) throw new Error("Token alınamadı.");
  setToken(token);
  return res;
}
