import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { api } from "../../shared/api/client";
import type { Paged } from "../../shared/types";
import { Modal } from "../../shared/components/Modal";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

interface Plan {
  id: number;
  name: string;
  duration: number;
  price: number;
}

interface Company {
  id: number;
  name: string;
}

export function AdminPlansPage() {
  const [editing, setEditing] = useState<Plan | null>();
  const [message, setMessage] = useState("");
  const action = useAsyncAction();

  const plans = useQuery({
    queryKey: ["plans"],
    queryFn: () => api<Plan[]>("/admin/plans"),
  });

  const companies = useQuery({
    queryKey: ["admin-companies"],
    queryFn: () => api<Paged<Company>>("/admin/companies?pageSize=50"),
  });

  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const f = Object.fromEntries(new FormData(e.currentTarget));

    setMessage("");
    const succeeded = await action.run("save-plan", async () => {
      await api(`/admin/plans${editing ? `/${editing.id}` : ""}`, {
        method: editing ? "PUT" : "POST",
        body: JSON.stringify({
          name: f.name,
          duration: Number(f.duration),
          price: Number(f.price),
        }),
      });
    });
    if (succeeded) {
      setEditing(undefined);
      await plans.refetch();
    }
  }

  async function remove(id: number) {
    await action.run(`remove-${id}`, async () => {
      await api(`/admin/plans/${id}`, { method: "DELETE" });
      await plans.refetch();
    });
  }

  async function assign(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.currentTarget));
    setMessage("");
    const succeeded = await action.run("assign-plan", async () => {
      await api(`/admin/companies/${f.companyId}/plan/${f.planId}`, {
        method: "PUT",
      });
    });
    if (succeeded) setMessage("Plan assigned successfully.");
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">SUBSCRIPTIONS</p>
          <h1>Company plans.</h1>
        </div>
        <button
          className="button"
          type="button"
          onClick={() => {
            action.clearError();
            setEditing(null);
          }}
        >
          <Plus /> Add plan
        </button>
      </div>
      {action.error && <p className="error">{action.error}</p>}
      {message && <p className="success">{message}</p>}
      <div className="plan-layout">
        <section className="grid">
          {plans.data?.map((x) => (
            <article className="card plan-card" key={x.id}>
              <p className="eyebrow">{x.duration} DAYS</p>
              <h2>{x.name}</h2>
              <strong>${x.price.toLocaleString()}</strong>
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
                <ActionButton
                  className="icon-button danger"
                  pending={action.isPending(`remove-${x.id}`)}
                  pendingLabel=""
                  aria-label={`Delete ${x.name} plan`}
                  onClick={() => remove(x.id)}
                >
                  <Trash2 />
                </ActionButton>
              </div>
            </article>
          ))}
        </section>
        <form className="card form-panel stack assign-panel" onSubmit={assign}>
          <div>
            <p className="eyebrow">ASSIGNMENT</p>
            <h3>Assign a plan</h3>
          </div>
          <div className="field">
            <label>Company</label>
            <select name="companyId" disabled={action.isPending()} required>
              {companies.data?.items.map((x) => (
                <option value={x.id} key={x.id}>
                  {x.name}
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label>Plan</label>
            <select name="planId" disabled={action.isPending()} required>
              {plans.data?.map((x) => (
                <option value={x.id} key={x.id}>
                  {x.name}
                </option>
              ))}
            </select>
          </div>
          <ActionButton
            className="button"
            type="submit"
            disabled={!companies.data?.items.length || !plans.data?.length}
            pending={action.isPending("assign-plan")}
            pendingLabel="Assigning…"
          >
            Assign plan
          </ActionButton>
        </form>
      </div>
      {editing !== undefined && (
        <Modal
          title={editing ? "Edit plan" : "Add plan"}
          onClose={() => !action.isPending() && setEditing(undefined)}
        >
          <form className="form-panel form-grid" onSubmit={save}>
            {action.error && <p className="error field full">{action.error}</p>}
            <div className="field full">
              <label>Name</label>
              <input name="name" defaultValue={editing?.name} minLength={2} maxLength={100} required />
            </div>
            <div className="field">
              <label>Duration in days</label>
              <input
                name="duration"
                type="number"
                defaultValue={editing?.duration ?? 30}
                min="1"
                max="3650"
                required
              />
            </div>
            <div className="field">
              <label>Price</label>
              <input
                name="price"
                type="number"
                step=".01"
                defaultValue={editing?.price ?? 0}
                min="0"
                max="1000000000"
                required
              />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("save-plan")}
              pendingLabel="Saving plan…"
            >
              Save plan
            </ActionButton>
          </form>
        </Modal>
      )}
    </div>
  );
}
