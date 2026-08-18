import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { expect, it } from "vitest";
import { CarCard } from "../shared/components/CarCard";
import type { Car } from "../shared/types";

it("shows the vehicle's core marketplace details", () => {
  const car: Car = {
    id: 1,
    companyId: 1,
    company: "Apex Motors",
    make: "BMW",
    model: "M4",
    year: 2025,
    type: "Coupe",
    gearType: "Automatic",
    fuelType: "Gasoline",
    seatsCount: 4,
    mileage: 5000,
    price: 78000,
    isAvailable: true,
    isForSale: true,
    inRent: false,
    rating: 4.8,
  };

  render(
    <MemoryRouter>
      <CarCard car={car} />
    </MemoryRouter>,
  );

  expect(screen.getByText("BMW M4")).toBeInTheDocument();

  expect(screen.getByText("$78,000")).toBeInTheDocument();

  expect(screen.getByRole("link", { name: /view details/i })).toHaveAttribute(
    "href",
    "/cars/1",
  );
});
