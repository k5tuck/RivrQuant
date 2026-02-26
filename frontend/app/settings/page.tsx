"use client";

type BadgeProps = {
  configured: boolean;
};

function StatusBadge({ configured }: BadgeProps) {
  if (configured) {
    return (
      <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-profit/15 text-profit dark:bg-profit/30 dark:text-profit">
        Configured
      </span>
    );
  }
  return (
    <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))]">
      Not configured
    </span>
  );
}

type SettingRowProps = {
  label: string;
  value: React.ReactNode;
};

function SettingRow({ label, value }: SettingRowProps) {
  return (
    <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
      <span className="text-sm text-[hsl(var(--muted-foreground))]">{label}</span>
      <span className="text-sm font-medium text-[hsl(var(--foreground))]">{value}</span>
    </div>
  );
}

type SectionCardProps = {
  title: string;
  children: React.ReactNode;
};

function SectionCard({ title, children }: SectionCardProps) {
  return (
    <div className="rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--card))] p-6 space-y-4">
      <h3 className="text-base font-semibold text-[hsl(var(--card-foreground))]">{title}</h3>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

export default function SettingsPage() {
  return (
    <div className="container mx-auto max-w-3xl px-4 py-8 space-y-6">
      <h2 className="text-2xl font-bold tracking-tight text-[hsl(var(--foreground))]">
        Settings
      </h2>

      {/* Broker Configuration */}
      <SectionCard title="Broker Configuration">
        <div className="space-y-4">
          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wider text-[hsl(var(--muted-foreground))]">
              Alpaca
            </p>
            <SettingRow label="API Key" value={<span className="font-mono tracking-widest">••••••••</span>} />
            <SettingRow label="API Secret" value={<span className="font-mono tracking-widest">••••••••</span>} />
            <SettingRow
              label="Mode"
              value={
                <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400">
                  Paper Trading
                </span>
              }
            />
          </div>

          <div className="border-t border-[hsl(var(--border))]" />

          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wider text-[hsl(var(--muted-foreground))]">
              Bybit
            </p>
            <SettingRow label="API Key" value={<span className="font-mono tracking-widest">••••••••</span>} />
            <SettingRow label="API Secret" value={<span className="font-mono tracking-widest">••••••••</span>} />
            <SettingRow
              label="Mode"
              value={
                <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-warning/15 text-warning dark:bg-warning/30 dark:text-warning">
                  Testnet
                </span>
              }
            />
          </div>
        </div>
      </SectionCard>

      {/* QuantConnect Integration */}
      <SectionCard title="QuantConnect Integration">
        <SettingRow label="User ID" value={<span className="font-mono tracking-widest">••••••••</span>} />
        <SettingRow label="Project IDs" value={<span className="font-mono tracking-widest">••••••••</span>} />
        <SettingRow label="Poll Interval" value="60 seconds" />
      </SectionCard>

      {/* AI Analysis */}
      <SectionCard title="AI Analysis">
        <SettingRow label="Claude Model" value="claude-sonnet-4-5-20250929" />
        <SettingRow label="Max Tokens" value="4096" />
        <SettingRow label="API Key" value={<span className="font-mono tracking-widest">••••••••</span>} />
      </SectionCard>

      {/* Notifications */}
      <SectionCard title="Notifications">
        <div className="space-y-4">
          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wider text-[hsl(var(--muted-foreground))]">
              SendGrid
            </p>
            <SettingRow label="Status" value={<StatusBadge configured={true} />} />
            <SettingRow label="API Key" value={<span className="font-mono tracking-widest">••••••••</span>} />
            <SettingRow label="From Email" value={<span className="font-mono tracking-widest">••••••••</span>} />
          </div>

          <div className="border-t border-[hsl(var(--border))]" />

          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wider text-[hsl(var(--muted-foreground))]">
              Twilio
            </p>
            <SettingRow label="Status" value={<StatusBadge configured={false} />} />
            <SettingRow label="Account SID" value={<StatusBadge configured={false} />} />
            <SettingRow label="Auth Token" value={<StatusBadge configured={false} />} />
            <SettingRow label="From Number" value={<StatusBadge configured={false} />} />
          </div>
        </div>
      </SectionCard>

      {/* System */}
      <SectionCard title="System">
        <SettingRow label="Database Provider" value="PostgreSQL" />
        <SettingRow
          label="Environment Mode"
          value={
            <span className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400">
              Development
            </span>
          }
        />
        <SettingRow label="API URL" value="http://localhost:8000" />
      </SectionCard>

      {/* Footer note */}
      <p className="text-sm text-[hsl(var(--muted-foreground))] text-center pb-4">
        Settings are configured via environment variables. See{" "}
        <code className="rounded bg-[hsl(var(--muted))] px-1.5 py-0.5 text-xs font-mono text-[hsl(var(--foreground))]">
          .env.example
        </code>{" "}
        for reference.
      </p>
    </div>
  );
}
