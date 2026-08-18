import { useCallback, useRef, useState } from "react";

function errorMessage(error: unknown) {
  return error instanceof Error ? error.message : "Something went wrong. Please try again.";
}

export function useAsyncAction() {
  const locked = useRef(false);
  const [pendingKey, setPendingKey] = useState<string | null>(null);
  const [error, setError] = useState("");

  const run = useCallback(async (key: string, action: () => Promise<unknown>) => {
    if (locked.current) return false;

    locked.current = true;
    setPendingKey(key);
    setError("");

    try {
      await action();
      return true;
    } catch (caught) {
      setError(errorMessage(caught));
      return false;
    } finally {
      locked.current = false;
      setPendingKey(null);
    }
  }, []);

  return {
    error,
    clearError: () => setError(""),
    pendingKey,
    isPending: (key?: string) =>
      key ? pendingKey === key : pendingKey !== null,
    run,
  };
}
