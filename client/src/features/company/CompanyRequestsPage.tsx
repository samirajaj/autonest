import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../shared/api/client";
import type { Paged, RequestItem } from "../../shared/types";
import { Modal } from "../../shared/components/Modal";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

export function CompanyRequestsPage() {
  const [state, setState] = useState("");
  const [approve, setApprove] = useState<RequestItem>();
  const action = useAsyncAction();

  const { data, refetch } = useQuery({
    queryKey: ["company-requests", state],
    queryFn: () =>
      api<Paged<RequestItem>>(
        `/company/requests${state ? `?state=${state}` : ""}`,
      ),
  });

  async function reject(id: number) {
    await action.run(`reject-${id}`, async () => {
      await api(`/company/requests/${id}/reject`, { method: "PUT" });
      await refetch();
    });
  }

  async function accept(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.currentTarget));
    const succeeded = await action.run("approve", async () => {
      await api(`/company/requests/${approve?.id}/approve`, {
        method: "PUT",
        body: JSON.stringify({
          deadline: f.deadline,
          paidAmount: Number(f.paidAmount),
        }),
      });
    });
    if (succeeded) {
      setApprove(undefined);
      await refetch();
    }
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">CUSTOMER DEMAND</p>
          <h1>Vehicle requests.</h1>
        </div>
        <select value={state} onChange={(e) => setState(e.target.value)}>
          <option value="">All statuses</option>
          {[
            "Pending",
            "Approved",
            "Rejected",
            "Cancelled",
            "Completed",
            "Obsoleted",
          ].map((x) => (
            <option key={x}>{x}</option>
          ))}
        </select>
      </div>
      {action.error && <p className="error">{action.error}</p>}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Vehicle</th>
              <th>Company</th>
              <th>Type</th>
              <th>Status</th>
              <th>Requested</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data?.items.map((x) => (
              <tr key={x.id}>
                <td>{x.car}</td>
                <td>{x.company}</td>
                <td>{x.type}</td>
                <td>
                  <span className={`status ${x.state}`}>{x.state}</span>
                </td>
                <td>{new Date(x.requestDate).toLocaleDateString()}</td>
                <td>
                  {x.state === "Pending" && (
                    <div className="toolbar">
                      <button
                        className="button"
                        type="button"
                        onClick={() => {
                          action.clearError();
                          setApprove(x);
                        }}
                      >
                        Approve
                      </button>
                      <ActionButton
                        className="button danger"
                        pending={action.isPending(`reject-${x.id}`)}
                        onClick={() => reject(x.id)}
                      >
                        Reject
                      </ActionButton>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {approve && (
        <Modal
          title={`Approve ${approve.car}`}
          onClose={() => !action.isPending() && setApprove(undefined)}
        >
          <form className="form-panel form-grid" onSubmit={accept}>
            {action.error && <p className="error field full">{action.error}</p>}
            <div className="field">
              <label>Completion deadline</label>
              <input
                name="deadline"
                type="date"
                min={new Date().toISOString().slice(0, 10)}
                disabled={action.isPending()}
                required
              />
            </div>
            <div className="field">
              <label>Paid amount</label>
              <input
                name="paidAmount"
                type="number"
                step="0.01"
                min="0.01"
                max="1000000000"
                disabled={action.isPending()}
                required
              />
            </div>
            <ActionButton
              className="button field full"
              type="submit"
              pending={action.isPending("approve")}
            >
              Approve request
            </ActionButton>
          </form>
        </Modal>
      )}
    </div>
  );
}
