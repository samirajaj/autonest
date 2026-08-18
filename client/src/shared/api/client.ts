const API_URL = "/api";

export class ApiError extends Error {
  constructor(
    public status: number,
    public detail: string,
  ) {
    super(detail);
  }
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("autonest_token");
  const headers = new Headers(init.headers);

  if (!(init.body instanceof FormData))
    headers.set("Content-Type", "application/json");

  if (token) headers.set("Authorization", `Bearer ${token}`);

  const response = await fetch(`${API_URL}${path}`, { ...init, headers });

  if (!response.ok) {
    let detail = response.statusText;

    try {
      const body = await response.json();
      detail = body.detail ?? body.title ?? detail;
    } catch {
      /* empty */
    }
    throw new ApiError(response.status, detail);
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}

export const imageUrl = (path?: string | null) =>
  path ? `${API_URL.replace(/\/api$/, "")}${path}` : "/placeholder.png";
