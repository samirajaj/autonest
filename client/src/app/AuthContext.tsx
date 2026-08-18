import {
  createContext,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { Role, Session } from "../shared/types";

type AuthValue = {
  session: Session | null;
  signIn: (s: Session) => void;
  signOut: () => void;
  hasRole: (...r: Role[]) => boolean;
};

const Context = createContext<AuthValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => {
    try {
      return JSON.parse(localStorage.getItem("autonest_session") || "null");
    } catch {
      return null;
    }
  });

  const value = useMemo(
    () => ({
      session,
      signIn: (s: Session) => {
        localStorage.setItem("autonest_token", s.token);
        localStorage.setItem("autonest_session", JSON.stringify(s));
        setSession(s);
      },
      signOut: () => {
        localStorage.removeItem("autonest_token");
        localStorage.removeItem("autonest_session");
        setSession(null);
      },
      hasRole: (...roles: Role[]) => !!session && roles.includes(session.role),
    }),
    [session],
  );
  return <Context.Provider value={value}>{children}</Context.Provider>;
}

export const useAuth = () => {
  const x = useContext(Context);
  if (!x) throw new Error("AuthProvider missing");
  return x;
};
