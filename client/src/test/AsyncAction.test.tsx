import { act, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { ActionButton } from "../shared/components/ActionButton";
import { useAsyncAction } from "../shared/hooks/useAsyncAction";

describe("async action protection", () => {
  it("allows only one request when a button is clicked repeatedly", async () => {
    let finishRequest: (() => void) | undefined;
    const request = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          finishRequest = resolve;
        }),
    );

    function Harness() {
      const action = useAsyncAction();
      const [completed, setCompleted] = useState(false);

      return (
        <>
          <ActionButton
            className="button"
            pending={action.isPending("save")}
            onClick={async () => {
              const succeeded = await action.run("save", request);
              if (succeeded) setCompleted(true);
            }}
          >
            Save
          </ActionButton>
          {completed && <span>Saved</span>}
        </>
      );
    }

    render(<Harness />);
    const button = screen.getByRole("button", { name: "Save" });

    fireEvent.click(button);
    fireEvent.click(button);

    expect(request).toHaveBeenCalledTimes(1);
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute("aria-busy", "true");

    await act(async () => finishRequest?.());

    await waitFor(() => expect(screen.getByText("Saved")).toBeInTheDocument());
    expect(screen.getByRole("button", { name: "Save" })).toBeEnabled();
  });
});
