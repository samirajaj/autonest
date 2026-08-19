import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../shared/api/client";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

interface Profile {
  id: number;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  city: string;
  area: string;
  points: number;
}

export function ProfilePage() {
  const { data } = useQuery({
    queryKey: ["profile"],
    queryFn: () => api<Profile>("/profile"),
  });

  const [msg, setMsg] = useState("");
  const action = useAsyncAction();

  async function update(e: FormEvent<HTMLFormElement>, path: string) {
    e.preventDefault();
    const body = Object.fromEntries(new FormData(e.currentTarget));

    setMsg("");
    const succeeded = await action.run(path, async () => {
      await api(`/profile/${path}`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
    });
    if (succeeded) setMsg("Your account was updated.");
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">YOUR ACCOUNT</p>
          <h1>
            {data?.firstName} {data?.lastName}
          </h1>
          <p className="muted">
            {data?.city}, {data?.area} · {data?.points ?? 0} points
          </p>
        </div>
      </div>
      {msg && <p className="success">{msg}</p>}
      {action.error && <p className="error">{action.error}</p>}
      <div className="profile-grid">
        <form
          className="card form-panel stack"
          onSubmit={(e) => update(e, "username")}
        >
          <h3>Username</h3>
          <div className="field">
            <label>Username</label>
            <input
              name="userName"
              defaultValue={data?.userName}
              autoComplete="username"
              minLength={3}
              maxLength={50}
              pattern="[A-Za-z0-9._-]+"
              disabled={action.isPending()}
              required
            />
          </div>
          <ActionButton className="button" type="submit" pending={action.isPending("username")}>
            Update username
          </ActionButton>
        </form>
        <form
          className="card form-panel stack"
          onSubmit={(e) => update(e, "email")}
        >
          <h3>Email address</h3>
          <div className="field">
            <label>New email</label>
            <input
              name="newEmail"
              type="email"
              defaultValue={data?.email}
              autoComplete="email"
              maxLength={254}
              disabled={action.isPending()}
              required
            />
          </div>
          <ActionButton className="button" type="submit" pending={action.isPending("email")}>
            Update email
          </ActionButton>
        </form>
        <form
          className="card form-panel stack"
          onSubmit={(e) => update(e, "password")}
        >
          <h3>Password</h3>
          <div className="field">
            <label>Current password</label>
            <input
              name="currentPassword"
              type="password"
              autoComplete="current-password"
              minLength={6}
              maxLength={128}
              disabled={action.isPending()}
              required
            />
          </div>
          <div className="field">
            <label>New password</label>
            <input
              name="newPassword"
              type="password"
              autoComplete="new-password"
              minLength={6}
              maxLength={128}
              disabled={action.isPending()}
              required
            />
          </div>
          <ActionButton className="button" type="submit" pending={action.isPending("password")}>
            Change password
          </ActionButton>
        </form>
      </div>
    </div>
  );
}
