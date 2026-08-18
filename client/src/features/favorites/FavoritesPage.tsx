import { useQuery } from "@tanstack/react-query";
import { api } from "../../shared/api/client";
import type { Car } from "../../shared/types";
import { CarCard } from "../../shared/components/CarCard";

export function FavoritesPage() {
  const { data = [], refetch } = useQuery({
    queryKey: ["favorites"],
    queryFn: () => api<Car[]>("/favorites"),
  });

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <p className="eyebrow">YOUR SHORTLIST</p>
          <h1>Saved vehicles.</h1>
        </div>
      </div>
      {data.length ? (
        <div className="grid">
          {data.map((x) => (
            <CarCard key={x.id} car={x} favorite onFavorite={refetch} />
          ))}
        </div>
      ) : (
        <div className="empty">Your favorites will appear here.</div>
      )}
    </div>
  );
}
