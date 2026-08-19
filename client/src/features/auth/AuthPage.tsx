import { useEffect, useState, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { api } from "../../shared/api/client";
import type { Session } from "../../shared/types";
import { useAuth } from "../../app/AuthContext";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return <span className="field-error">{message}</span>;
}

export function AuthPage() {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [done, setDone] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const action = useAsyncAction();

  const { signIn } = useAuth();
  const navigate = useNavigate();

  function clearField(name: string) {
    setFieldErrors((prev) => {
      if (!(name in prev)) return prev;
      const next = { ...prev };
      delete next[name];
      return next;
    });
  }

  function validate(data: Record<string, unknown>): boolean {
    const errors: Record<string, string> = {};

    if (mode === "register") {
      if (!String(data.firstName || "").trim()) errors.firstName = "First name is required.";
      else if (String(data.firstName).trim().length < 2) errors.firstName = "Must be at least 2 characters.";

      if (!String(data.lastName || "").trim()) errors.lastName = "Last name is required.";
      else if (String(data.lastName).trim().length < 2) errors.lastName = "Must be at least 2 characters.";

      if (!String(data.userName || "").trim()) errors.userName = "Username is required.";
      else if (String(data.userName).trim().length < 3) errors.userName = "Must be at least 3 characters.";
      else if (String(data.userName).trim().length > 50) errors.userName = "Must be at most 50 characters.";
      else if (!/^[A-Za-z0-9._-]+$/.test(String(data.userName).trim())) errors.userName = "Use letters, numbers, dots, underscores, or hyphens.";

      if (!data.birthDate) errors.birthDate = "Birth date is required.";

      if (!data.areaName || !String(data.areaName).trim()) errors.areaName = "Area is required.";
      else if (String(data.areaName).trim().length < 2) errors.areaName = "Must be at least 2 characters.";

      if (!data.password) errors.password = "Password is required.";
      else if (String(data.password).length < 6) errors.password = "Must be at least 6 characters.";
      else if (String(data.password).length > 128) errors.password = "Must be at most 128 characters.";

      if (!data.confirmPassword) errors.confirmPassword = "Please confirm your password.";
      else if (data.password !== data.confirmPassword) errors.confirmPassword = "Passwords do not match.";
    }

    if (!data.email || !String(data.email).trim()) errors.email = "Email is required.";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(data.email).trim())) errors.email = "Enter a valid email address.";

    if (mode === "login") {
      if (!data.password) errors.password = "Password is required.";
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    action.clearError();
    setDone("");
    const data = Object.fromEntries(new FormData(e.currentTarget));

    if (!validate(data)) return;

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
            firstName: data.firstName,
            lastName: data.lastName,
            userName: data.userName,
            email: data.email,
            password: data.password,
            cityId: Number(data.cityId),
            birthDate: data.birthDate,
            areaName: data.areaName,
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
                setFieldErrors({});
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
                setFieldErrors({});
                setMode("register");
              }}
            >
              Register
            </button>
          </div>
          {action.error && <p className="error">{action.error}</p>}
          {done && <p className="success">{done}</p>}
          <form className="form-grid" onSubmit={submit} noValidate>
            {mode === "register" && (
              <>
                <div className="field">
                  <label>First name</label>
                  <input
                    name="firstName"
                    autoComplete="given-name"
                    disabled={action.isPending()}
                    className={fieldErrors.firstName ? "has-error" : ""}
                    onChange={() => clearField("firstName")}
                  />
                  <FieldError message={fieldErrors.firstName} />
                </div>
                <div className="field">
                  <label>Last name</label>
                  <input
                    name="lastName"
                    autoComplete="family-name"
                    disabled={action.isPending()}
                    className={fieldErrors.lastName ? "has-error" : ""}
                    onChange={() => clearField("lastName")}
                  />
                  <FieldError message={fieldErrors.lastName} />
                </div>
                <div className="field">
                  <label>Username</label>
                  <input
                    name="userName"
                    autoComplete="username"
                    disabled={action.isPending()}
                    className={fieldErrors.userName ? "has-error" : ""}
                    onChange={() => clearField("userName")}
                  />
                  <FieldError message={fieldErrors.userName} />
                </div>
                <div className="field">
                  <label>Birth date</label>
                  <input
                    name="birthDate"
                    type="date"
                    min="1900-01-01"
                    max={new Date().toISOString().slice(0, 10)}
                    autoComplete="bday"
                    disabled={action.isPending()}
                    className={fieldErrors.birthDate ? "has-error" : ""}
                    onChange={() => clearField("birthDate")}
                  />
                  <FieldError message={fieldErrors.birthDate} />
                </div>
                <div className="field">
                  <label>City</label>
                  <select
                    name="cityId"
                    defaultValue="1"
                    disabled={action.isPending()}
                  >
                    <option value="1">Damascus</option>
                    <option value="2">Aleppo</option>
                    <option value="3">Homs</option>
                    <option value="4">Latakia</option>
                  </select>
                </div>
                <div className="field">
                  <label>Area</label>
                  <input
                    name="areaName"
                    autoComplete="address-level2"
                    disabled={action.isPending()}
                    className={fieldErrors.areaName ? "has-error" : ""}
                    onChange={() => clearField("areaName")}
                  />
                  <FieldError message={fieldErrors.areaName} />
                </div>
              </>
            )}
            <div className="field full">
              <label>Email</label>
              <input
                name="email"
                type="email"
                autoComplete="email"
                disabled={action.isPending()}
                className={fieldErrors.email ? "has-error" : ""}
                onChange={() => clearField("email")}
              />
              <FieldError message={fieldErrors.email} />
            </div>
            <div className="field full">
              <label>Password</label>
              <input
                name="password"
                type="password"
                autoComplete={mode === "login" ? "current-password" : "new-password"}
                disabled={action.isPending()}
                className={fieldErrors.password ? "has-error" : ""}
                onChange={() => clearField("password")}
              />
              <FieldError message={fieldErrors.password} />
            </div>
            {mode === "register" && (
              <div className="field full">
                <label>Confirm password</label>
                <input
                  name="confirmPassword"
                  type="password"
                  autoComplete="new-password"
                  disabled={action.isPending()}
                  className={fieldErrors.confirmPassword ? "has-error" : ""}
                  onChange={() => clearField("confirmPassword")}
                />
                <FieldError message={fieldErrors.confirmPassword} />
              </div>
            )}
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("auth")}
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
