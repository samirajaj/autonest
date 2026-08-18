import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { api } from "../../shared/api/client";
import type { Dashboard } from "../../shared/types";
import { useAuth } from "../../app/AuthContext";

export function DashboardPage() {
  const { session } = useAuth();
  const [year, setYear] = useState(new Date().getFullYear());

  const root = session?.role === "Admin" ? "/admin" : "/company";

  const { data } = useQuery({
    queryKey: ["dashboard", root, year],
    queryFn: () => api<Dashboard>(`${root}/dashboard?year=${year}`),
  });

  const chart = (data?.quarterlyProfits ?? []).map((value, i) => ({
    quarter: `Q${i + 1}`,
    value,
  }));

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">{session?.role?.toUpperCase()} OVERVIEW</p>
          <h1>Good to see you, {session?.displayName}.</h1>
          <p className="muted">
            A clear view of what is moving across AutoNest.
          </p>
        </div>
        <div className="field">
          <label>Reporting year</label>
          <select
            value={year}
            onChange={(e) => setYear(Number(e.target.value))}
          >
            {[0, 1, 2, 3].map((x) => (
              <option key={x}>{new Date().getFullYear() - x}</option>
            ))}
          </select>
        </div>
      </div>
      <section className="metric-grid">
        {data?.metrics.map((x) => (
          <article className="card metric" key={x.label}>
            <span>{x.label}</span>
            <strong>
              {x.format === "currency"
                ? `$${x.value.toLocaleString()}`
                : x.value.toLocaleString()}
            </strong>
          </article>
        ))}
      </section>
      <section className="card chart-panel">
        <div className="page-head compact">
          <div>
            <p className="eyebrow">PERFORMANCE</p>
            <h3>Quarterly earnings</h3>
          </div>
        </div>
        <ResponsiveContainer width="100%" height="82%">
          <BarChart data={chart}>
            <CartesianGrid stroke="#272e38" vertical={false} />
            <XAxis dataKey="quarter" stroke="#7f8998" />
            <YAxis stroke="#7f8998" />
            <Tooltip
              contentStyle={{
                background: "#151a22",
                border: "1px solid #2b333e",
                borderRadius: 12,
              }}
            />
            <Bar dataKey="value" fill="#d9ff43" radius={[8, 8, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </section>
    </div>
  );
}
