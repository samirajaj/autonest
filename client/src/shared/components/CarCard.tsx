import { Gauge, Heart, Star, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { api, imageUrl } from "../api/client";
import type { Car } from "../types";
import { ActionButton } from "./ActionButton";
import { useAsyncAction } from "../hooks/useAsyncAction";

export function CarCard({
  car,
  favorite = false,
  onFavorite,
}: {
  car: Car;
  favorite?: boolean;
  onFavorite?: () => void;
}) {
  const action = useAsyncAction();

  async function toggle() {
    await action.run("favorite", async () => {
      await api(`/favorites/${car.id}`, { method: favorite ? "DELETE" : "POST" });
      onFavorite?.();
    });
  }

  return (
    <article className="card car-card">
      <img
        className="car-image"
        src={imageUrl(car.imageUrl)}
        alt={`${car.make} ${car.model}`}
        onError={(event) => {
          event.currentTarget.onerror = null;
          event.currentTarget.src = "/placeholder.png";
        }}
      />
      <div className="car-card-body">
        {action.error && <p className="error compact-message">{action.error}</p>}
        <div className="car-card-top">
          <div>
            <p className="eyebrow">{car.company}</p>
            <h3>
              {car.make} {car.model}
            </h3>
          </div>
          <span className="price">${car.price.toLocaleString()}</span>
        </div>
        <div className="meta">
          <span className="chip">{car.year}</span>
          <span className="chip">
            <Gauge size={13} /> {car.mileage.toLocaleString()} km
          </span>
          <span className="chip">
            <Users size={13} /> {car.seatsCount}
          </span>
        </div>
        <div className="car-actions">
          <span className="rating">
            <Star size={15} fill="currentColor" /> {car.rating.toFixed(1)}
          </span>
          <div className="toolbar">
            {onFavorite && (
              <ActionButton
                className="icon-button"
                aria-label={favorite ? "Remove from favorites" : "Add to favorites"}
                pending={action.isPending("favorite")}
                pendingLabel=""
                onClick={toggle}
              >
                <Heart size={18} fill={favorite ? "currentColor" : "none"} />
              </ActionButton>
            )}
            <Link className="button secondary" to={`/cars/${car.id}`}>
              View details
            </Link>
          </div>
        </div>
      </div>
    </article>
  );
}
