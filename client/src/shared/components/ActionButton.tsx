import { LoaderCircle } from "lucide-react";
import type { ButtonHTMLAttributes, ReactNode } from "react";

type ActionButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  pending?: boolean;
  pendingLabel?: string;
  children: ReactNode;
};

export function ActionButton({
  pending = false,
  pendingLabel = "Working…",
  disabled,
  children,
  type = "button",
  ...props
}: ActionButtonProps) {
  return (
    <button
      {...props}
      type={type}
      disabled={disabled || pending}
      aria-busy={pending}
    >
      {pending && <LoaderCircle className="button-spinner" size={17} />}
      {pending ? pendingLabel : children}
    </button>
  );
}
