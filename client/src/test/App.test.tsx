import { render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it } from "vitest";
import { App } from "../app/App";

describe("AutoNest routing", () => {
  beforeEach(() => {
    localStorage.clear();
    history.pushState({}, "", "/");
  });

  it("renders the premium customer landing page", () => {
    render(<App />);
    expect(screen.getByText(/Find the car that/i)).toBeInTheDocument();
  });

  it("protects customer routes", () => {
    history.pushState({}, "", "/favorites");
    render(<App />);
    expect(screen.getByText("Sign in to continue")).toBeInTheDocument();
  });

  it("shows only public navigation to guests", () => {
    render(<App />);
    const navigation = screen.getByRole("navigation", { name: /primary/i });

    expect(within(navigation).getByRole("link", { name: "Browse cars" })).toBeInTheDocument();
    expect(within(navigation).getByRole("link", { name: "Privacy" })).toBeInTheDocument();
    expect(within(navigation).getByRole("link", { name: "Sign in" })).toBeInTheDocument();
    expect(within(navigation).queryByRole("link", { name: "Favorites" })).not.toBeInTheDocument();
    expect(within(navigation).queryByRole("link", { name: "My requests" })).not.toBeInTheDocument();
    expect(within(navigation).queryByRole("link", { name: "Profile" })).not.toBeInTheDocument();
  });

  it.each([
    ["Customer", ["Browse cars", "Favorites", "My requests", "Profile", "Privacy"]],
    ["Company", ["Dashboard", "Vehicles", "Requests", "Privacy"]],
    ["Admin", ["Dashboard", "Users", "Plans", "Points", "Privacy"]],
  ] as const)("shows the correct %s navigation", (role, expectedLinks) => {
    localStorage.setItem(
      "autonest_session",
      JSON.stringify({
        token: "test-token",
        expiresAt: "2099-01-01T00:00:00Z",
        role,
        displayName: `${role} user`,
      }),
    );

    render(<App />);
    const navigation = screen.getByRole("navigation", { name: /primary/i });
    const labels = within(navigation)
      .getAllByRole("link")
      .map((link) => link.textContent?.trim());

    expect(labels).toEqual(expectedLinks);
    expect(within(navigation).getByRole("button", { name: "Sign out" })).toBeInTheDocument();
  });
});
