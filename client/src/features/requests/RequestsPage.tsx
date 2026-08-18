import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../../shared/api/client";
import type { RequestItem, Transaction } from "../../shared/types";
import { ActionButton } from "../../shared/components/ActionButton";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";

export function RequestsPage() {
  const [tab, setTab] = useState<"requests" | "transactions">("requests");
  const action = useAsyncAction();

  const requests = useQuery({
    queryKey: ["requests"],
    queryFn: () => api<RequestItem[]>("/requests"),
  });

  const transactions = useQuery({
    queryKey: ["transactions"],
    queryFn: () => api<Transaction[]>("/transactions"),
  });

  async function cancel(id: number) {
    await action.run(`cancel-${id}`, async () => {
      await api(`/requests/${id}`, { method: "DELETE" });
      await requests.refetch();
    });
  }

  async function rate(id: number, value: number) {
    if (value < 1 || value > 5) return;
    await action.run(`rate-${id}`, async () => {
      await api(`/transactions/${id}/rating`, {
        method: "PUT",
        body: JSON.stringify({ value }),
      });
      await transactions.refetch();
    });
  }

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">ACTIVITY</p>
          <h1>Requests & transactions.</h1>
        </div>
        <div className="tabs">
          <button
            className={tab === "requests" ? "active" : ""}
            onClick={() => setTab("requests")}
          >
            Requests
          </button>
          <button
            className={tab === "transactions" ? "active" : ""}
            onClick={() => setTab("transactions")}
          >
            Transactions
          </button>
        </div>
      </div>
      {action.error && <p className="error">{action.error}</p>}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              {tab === "requests" ? (
                <>
                  <th>Vehicle</th>
                  <th>Company</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Date</th>
                  <th />
                </>
              ) : (
                <>
                  <th>Vehicle</th>
                  <th>Amount</th>
                  <th>Type</th>
                  <th>Date</th>
                  <th>Rating</th>
                </>
              )}
            </tr>
          </thead>
          <tbody>
            {tab === "requests"
              ? requests.data?.map((x) => (
                  <tr key={x.id}>
                    <td>{x.car}</td>
                    <td>{x.company}</td>
                    <td>{x.type}</td>
                    <td>
                      <span className={`status ${x.state}`}>{x.state}</span>
                    </td>
                    <td>{new Date(x.requestDate).toLocaleDateString()}</td>
                    <td>
                      {["Pending", "Approved"].includes(x.state) && (
                        <ActionButton
                          className="button danger"
                          pending={action.isPending(`cancel-${x.id}`)}
                          pendingLabel="Cancelling…"
                          onClick={() => cancel(x.id)}
                        >
                          Cancel
                        </ActionButton>
                      )}
                    </td>
                  </tr>
                ))
              : transactions.data?.map((x) => (
                  <tr key={x.id}>
                    <td>{x.car}</td>
                    <td>${x.paidAmount.toLocaleString()}</td>
                    <td>{x.state}</td>
                    <td>{new Date(x.listingDate).toLocaleDateString()}</td>
                    <td>
                      <select
                        value={x.rating ?? ""}
                        disabled={action.isPending(`rate-${x.id}`)}
                        aria-label={`Rate ${x.car}`}
                        onChange={(e) => rate(x.id, Number(e.target.value))}
                      >
                        <option value="">Rate</option>
                        {[1, 2, 3, 4, 5].map((n) => (
                          <option key={n}>{n}</option>
                        ))}
                      </select>
                    </td>
                  </tr>
                ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
