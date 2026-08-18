import { useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { api } from "../../shared/api/client";
import { Modal } from "../../shared/components/Modal";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

interface Range {
  id: number;
  minAmount: number;
  maxAmount: number;
  point: number;
}

export function AdminPointsPage() {
  const [editing, setEditing] = useState<Range | null>();
  const action = useAsyncAction();

  const { data = [], refetch } = useQuery({
    queryKey: ["point-ranges"],
    queryFn: () => api<Range[]>("/admin/point-ranges"),
  });

  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.currentTarget));
    const succeeded = await action.run("save-range", async () => {
      if (Number(f.maxAmount) < Number(f.minAmount))
        throw new Error("Maximum amount must be greater than or equal to the minimum amount.");
      await api(`/admin/point-ranges${editing ? `/${editing.id}` : ""}`, {
        method: editing ? "PUT" : "POST",
        body: JSON.stringify({
          minAmount: Number(f.minAmount),
          maxAmount: Number(f.maxAmount),
          point: Number(f.point),
        }),
      });
    });
    if (succeeded) {
      setEditing(undefined);
      await refetch();
    }
  }

  async function remove(id: number) {
    await action.run(`remove-${id}`, async () => {
      await api(`/admin/point-ranges/${id}`, { method: "DELETE" });
      await refetch();
    });
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">LOYALTY RULES</p>
          <h1>Point ranges.</h1>
          <p className="muted">
            Reward customers based on completed transaction value.
          </p>
        </div>
        <button
          className="button"
          type="button"
          onClick={() => {
            action.clearError();
            setEditing(null);
          }}
        >
          <Plus /> Add range
        </button>
      </div>
      {action.error && <p className="error">{action.error}</p>}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Minimum amount</th>
              <th>Maximum amount</th>
              <th>Points awarded</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data.map((x) => (
              <tr key={x.id}>
                <td>${x.minAmount.toLocaleString()}</td>
                <td>${x.maxAmount.toLocaleString()}</td>
                <td>{x.point}</td>
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
                    <ActionButton
                      className="icon-button danger"
                      pending={action.isPending(`remove-${x.id}`)}
                      pendingLabel=""
                      aria-label={`Delete point range ${x.minAmount} to ${x.maxAmount}`}
                      onClick={() => remove(x.id)}
                    >
                      <Trash2 />
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
          title={editing ? "Edit point range" : "Add point range"}
          onClose={() => !action.isPending() && setEditing(undefined)}
        >
          <form className="form-panel form-grid" onSubmit={save}>
            {action.error && <p className="error field full">{action.error}</p>}
            <div className="field">
              <label>Minimum amount</label>
              <input
                name="minAmount"
                type="number"
                defaultValue={editing?.minAmount ?? 0}
                min="0"
                max="1000000000"
                required
              />
            </div>
            <div className="field">
              <label>Maximum amount</label>
              <input
                name="maxAmount"
                type="number"
                defaultValue={editing?.maxAmount ?? 0}
                min="0"
                max="1000000000"
                required
              />
            </div>
            <div className="field full">
              <label>Points</label>
              <input
                name="point"
                type="number"
                defaultValue={editing?.point ?? 0}
                min="1"
                max="1000000"
                required
              />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("save-range")}
              pendingLabel="Saving range…"
            >
              Save range
            </ActionButton>
          </form>
        </Modal>
      )}
    </div>
  );
}
