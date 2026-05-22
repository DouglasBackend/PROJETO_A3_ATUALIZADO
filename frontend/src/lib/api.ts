const API_URL = "http://localhost:5093/api";

export async function fetchApi(endpoint: string, options: RequestInit = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...options.headers,
  } as Record<string, string>;

  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers,
    credentials: "include",
  });

  if (!response.ok) {
    let errorData;
    try {
      errorData = await response.json();
    } catch {
      errorData = {};
    }
    throw new Error(errorData.mensagem || errorData.error || errorData.message || "Erro na requisição");
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
