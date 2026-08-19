import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { Lock, LockOpen, Plus } from "lucide-react";
import { api } from "../../shared/api/client";
import type { Car, Paged } from "../../shared/types";
import { Modal } from "../../shared/components/Modal";
import { CarCard } from "../../shared/components/CarCard";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

interface Customer {
  id: number;
  userId: string;
  name: string;
  email: string;
  locked: boolean;
  points: number;
}

interface Company {
  id: number;
  userId: string;
  name: string;
  email: string;
  locked: boolean;
  planId?: number;
}

export function AdminUsersPage() {
  const [tab, setTab] = useState<"customers" | "companies">("customers");
  const [editing, setEditing] = useState<Customer | Company | null>();
  const [vehicles, setVehicles] = useState<Car[]>();
  const action = useAsyncAction();

  const { data, refetch } = useQuery({
    queryKey: ["admin-users", tab],
    queryFn: () => api<Paged<Customer | Company>>(`/admin/${tab}`),
  });

  async function lock(x: Customer | Company) {
    await action.run(`lock-${x.userId}`, async () => {
      await api(`/admin/users/${x.userId}/lock`, {
        method: x.locked ? "DELETE" : "PUT",
      });
      await refetch();
    });
  }

  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.currentTarget));
    const isCompany = tab === "companies";
    const body = isCompany
      ? {
          email: f.email,
          userName: f.userName,
          password: f.password || null,
          name: f.name,
          cityId: Number(f.cityId),
          areaName: f.areaName,
          contacts: f.contact
            ? [{ type: "PhoneNumber", value: f.contact }]
            : [],
        }
      : {
          email: f.email,
          userName: f.userName,
          password: f.password || null,
          firstName: f.firstName,
          lastName: f.lastName,
          birthDate: f.birthDate,
          cityId: Number(f.cityId),
          areaName: f.areaName,
        };

    const succeeded = await action.run("save-account", async () => {
      await api(`/admin/${tab}${editing ? `/${editing.id}` : ""}`, {
        method: editing ? "PUT" : "POST",
        body: JSON.stringify(body),
      });
    });
    if (succeeded) {
      setEditing(undefined);
      await refetch();
    }
  }

  async function viewCars(id: number) {
    await action.run(`vehicles-${id}`, async () => {
      const x = await api<Paged<Car>>(`/admin/companies/${id}/cars`);
      setVehicles(x.items);
    });
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">ACCOUNT MANAGEMENT</p>
          <h1>People & companies.</h1>
        </div>
        <div className="toolbar">
          <div className="tabs">
            <button
              className={tab === "customers" ? "active" : ""}
              onClick={() => setTab("customers")}
            >
              Customers
            </button>
            <button
              className={tab === "companies" ? "active" : ""}
              onClick={() => setTab("companies")}
            >
              Companies
            </button>
          </div>
          <button
            className="button"
            type="button"
            onClick={() => {
              action.clearError();
              setEditing(null);
            }}
          >
            <Plus /> Add {tab === "customers" ? "customer" : "company"}
          </button>
        </div>
      </div>
      {action.error && <p className="error">{action.error}</p>}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>{tab === "customers" ? "Points" : "Plan"}</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data?.items.map((x) => (
              <tr key={x.id}>
                <td>{x.name}</td>
                <td>{x.email}</td>
                <td>{"points" in x ? x.points : (x.planId ?? "None")}</td>
                <td>
                  <span
                    className={`status ${x.locked ? "Rejected" : "Approved"}`}
                  >
                    {x.locked ? "Locked" : "Active"}
                  </span>
                </td>
                <td>
                  <div className="toolbar">
                    <button
                      className="button secondary"
                      onClick={() => {
                        action.clearError();
                        setEditing(x);
                      }}
                    >
                      Edit
                    </button>
                    {tab === "companies" && (
                      <ActionButton
                        className="button secondary"
                        pending={action.isPending(`vehicles-${x.id}`)}
                        onClick={() => viewCars(x.id)}
                      >
                        Vehicles
                      </ActionButton>
                    )}
                    <ActionButton
                      className="icon-button"
                      pending={action.isPending(`lock-${x.userId}`)}
                      aria-label={x.locked ? `Unlock ${x.name}` : `Lock ${x.name}`}
                      onClick={() => lock(x)}
                    >
                      {x.locked ? <LockOpen /> : <Lock />}
                    </ActionButton>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {editing !== undefined && (
        <Modal
          title={`${editing ? "Edit" : "Add"} ${tab === "customers" ? "customer" : "company"}`}
          onClose={() => !action.isPending() && setEditing(undefined)}
        >
          <form className="form-panel form-grid" onSubmit={save}>
            {action.error && <p className="error field full">{action.error}</p>}
            {tab === "customers" ? (
              <>
                <div className="field">
                  <label>First name</label>
                  <input
                    name="firstName"
                    defaultValue={editing?.name.split(" ")[0]}
                    minLength={2}
                    maxLength={100}
                    autoComplete="given-name"
                    required
                  />
                </div>
                <div className="field">
                  <label>Last name</label>
                  <input
                    name="lastName"
                    defaultValue={editing?.name.split(" ").slice(1).join(" ")}
                    minLength={2}
                    maxLength={100}
                    autoComplete="family-name"
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
                    required={!editing}
                  />
                </div>
              </>
            ) : (
              <>
                <div className="field full">
                  <label>Company name</label>
                  <input name="name" defaultValue={editing?.name} minLength={2} maxLength={150} required />
                </div>
                <div className="field full">
                  <label>Phone/contact</label>
                  <input name="contact" type="tel" autoComplete="tel" maxLength={50} />
                </div>
              </>
            )}
            <div className="field">
              <label>Email</label>
              <input
                name="email"
                type="email"
                defaultValue={editing?.email}
                autoComplete="email"
                maxLength={254}
                required
              />
            </div>
            <div className="field">
              <label>Username</label>
              <input
                name="userName"
                defaultValue={editing?.email.split("@")[0]}
                autoComplete="username"
                minLength={3}
                maxLength={50}
                pattern="[A-Za-z0-9._-]+"
                required
              />
            </div>
            <div className="field">
              <label>Password {editing && "(optional)"}</label>
              <input
                name="password"
                type="password"
                autoComplete="new-password"
                minLength={6}
                maxLength={128}
                required={!editing}
              />
            </div>
            <div className="field">
              <label>City</label>
              <select name="cityId" required>
                <option value="1">Damascus</option>
                <option value="2">Aleppo</option>
                <option value="3">Homs</option>
                <option value="4">Latakia</option>
              </select>
            </div>
            <div className="field full">
              <label>Area</label>
              <input name="areaName" minLength={2} maxLength={100} required />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("save-account")}
            >
              Save account
            </ActionButton>
          </form>
        </Modal>
      )}
      {vehicles && (
        <Modal title="Company vehicles" onClose={() => setVehicles(undefined)}>
          <div className="form-panel stack">
            {vehicles.length ? (
              vehicles.map((x) => <CarCard car={x} key={x.id} />)
            ) : (
              <div className="empty">No active vehicles.</div>
            )}
          </div>
        </Modal>
      )}
    </div>
  );
}
