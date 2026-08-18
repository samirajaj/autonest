import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { api } from "../../shared/api/client";
import type { Session } from "../../shared/types";
import { useAuth } from "../../app/AuthContext";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

export function AuthPage() {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [done, setDone] = useState("");
  const action = useAsyncAction();

  const { signIn } = useAuth();
  const navigate = useNavigate();

  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    action.clearError();
    setDone("");
    const data = Object.fromEntries(new FormData(e.currentTarget));
    await action.run("auth", async () => {
      if (mode === "login") {
        const session = await api<Session>("/auth/login", {
          method: "POST",
          body: JSON.stringify({ email: data.email, password: data.password }),
        });
        signIn(session);
        navigate(session.role === "Customer" ? "/cars" : "/dashboard");
      } else {
        await api("/auth/register", {
          method: "POST",
          body: JSON.stringify({
            ...data,
            cityId: Number(data.cityId),
            birthDate: data.birthDate,
          }),
        });
        setDone(
          "Account created. Check your email to confirm it before signing in.",
        );
      }
    });
  }

  return (
    <div className="auth-page">
      <section className="auth-visual">
        <div>
          <p className="eyebrow">MEMBERSHIP</p>
          <h1>
            Your next drive
            <br />
            starts here.
          </h1>
          <p>
            Save vehicles, place rental or purchase requests, and keep every
            transaction in one place.
          </p>
        </div>
      </section>
      <section className="auth-form">
        <div className="auth-form-inner">
          <p className="eyebrow">WELCOME TO AUTONEST</p>
          <h2>
            {mode === "login" ? "Sign in to continue" : "Create your account"}
          </h2>
          <div className="tabs">
            <button
              className={mode === "login" ? "active" : ""}
              type="button"
              disabled={action.isPending()}
              onClick={() => {
                action.clearError();
                setDone("");
                setMode("login");
              }}
            >
              Sign in
            </button>
            <button
              className={mode === "register" ? "active" : ""}
              type="button"
              disabled={action.isPending()}
              onClick={() => {
                action.clearError();
                setDone("");
                setMode("register");
              }}
            >
              Register
            </button>
          </div>
          {action.error && <p className="error">{action.error}</p>}
          {done && <p className="success">{done}</p>}
          <form className="form-grid" onSubmit={submit}>
            {mode === "register" && (
              <>
                <div className="field">
                  <label>First name</label>
                  <input name="firstName" autoComplete="given-name" minLength={2} maxLength={100} required />
                </div>
                <div className="field">
                  <label>Last name</label>
                  <input name="lastName" autoComplete="family-name" minLength={2} maxLength={100} required />
                </div>
                <div className="field">
                  <label>Username</label>
                  <input
                    name="userName"
                    autoComplete="username"
                    minLength={3}
                    maxLength={50}
                    pattern="[A-Za-z0-9._-]+"
                    title="Use letters, numbers, dots, underscores, or hyphens."
                    required
                  />
                </div>
                <div className="field">
                  <label>Birth date</label>
                  <input
                    name="birthDate"
                    type="date"
                    min="1900-01-01"
                    max={new Date().toISOString().slice(0, 10)}
                    autoComplete="bday"
                    required
                  />
                </div>
                <div className="field">
                  <label>City</label>
                  <select name="cityId" defaultValue="1" required>
                    <option value="1">Damascus</option>
                    <option value="2">Aleppo</option>
                    <option value="3">Homs</option>
                    <option value="4">Latakia</option>
                  </select>
                </div>
                <div className="field">
                  <label>Area</label>
                  <input name="areaName" autoComplete="address-level2" minLength={2} maxLength={100} required />
                </div>
              </>
            )}
            <div className="field full">
              <label>Email</label>
              <input name="email" type="email" autoComplete="email" maxLength={254} required />
            </div>
            <div className="field full">
              <label>Password</label>
              <input
                name="password"
                type="password"
                autoComplete={mode === "login" ? "current-password" : "new-password"}
                minLength={6}
                maxLength={128}
                required
              />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("auth")}
              pendingLabel={mode === "login" ? "Signing in…" : "Creating account…"}
            >
              {mode === "login" ? "Sign in" : "Create account"}
            </ActionButton>
          </form>
          {mode === "login" && (
            <button className="text-button" type="button" onClick={() => setMode("register")}>
              New to AutoNest? Create an account
            </button>
          )}
        </div>
      </section>
    </div>
  );
}

export function ConfirmEmailPage() {
  const [params] = useSearchParams();
  const [state, setState] = useState("Confirming your email…");

  useEffect(() => {
    api(
      `/auth/confirm-email?userId=${encodeURIComponent(params.get("userId") || "")}&token=${encodeURIComponent(params.get("token") || "")}`,
    )
      .then(() => setState("Email confirmed. You can now sign in."))
      .catch((e) => setState(e.message));
  }, [params]);
  return (
    <div className="page">
      <h1>{state}</h1>
    </div>
  );
}
