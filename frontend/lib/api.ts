import type {
  DashboardDto, BacktestSummaryDto, BacktestDetailDto, PositionDto,
  OrderDto, AnalysisReportDto, AlertRuleDto, AlertEventDto, StrategyDto, PortfolioDto
} from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

async function fetchApi<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    headers: { "Content-Type": "application/json", ...options?.headers },
    ...options,
  });
  if (!res.ok) throw new Error(`API error: ${res.status} ${res.statusText}`);
  return res.json();
}

export const api = {
  dashboard: {
    get: () => fetchApi<DashboardDto>("/api/dashboard"),
    portfolio: () => fetchApi<PortfolioDto>("/api/dashboard/portfolio"),
  },
  backtests: {
    list: () => fetchApi<BacktestSummaryDto[]>("/api/backtest"),
    get: (id: string) => fetchApi<BacktestDetailDto>(`/api/backtest/${id}`),
    analyze: (id: string) => fetchApi<AnalysisReportDto>(`/api/backtest/${id}/analyze`, { method: "POST" }),
    compare: (ids: string[]) => fetchApi<unknown>("/api/backtest/compare", { method: "POST", body: JSON.stringify(ids) }),
  },
  trading: {
    positions: () => fetchApi<PositionDto[]>("/api/trading/positions"),
    orders: () => fetchApi<OrderDto[]>("/api/trading/orders"),
    placeOrder: (order: { symbol: string; side: string; type: string; quantity: number; limitPrice?: number; stopPrice?: number; assetClass: string }) =>
      fetchApi<OrderDto>("/api/trading/orders", { method: "POST", body: JSON.stringify(order) }),
    cancelOrder: (id: string) => fetchApi<void>(`/api/trading/orders/${id}`, { method: "DELETE" }),
    closeAll: () => fetchApi<void>("/api/trading/close-all", { method: "POST" }),
  },
  analysis: {
    list: () => fetchApi<AnalysisReportDto[]>("/api/analysis"),
    get: (id: string) => fetchApi<AnalysisReportDto>(`/api/analysis/${id}`),
    run: (backtestId: string) => fetchApi<AnalysisReportDto>(`/api/analysis/${backtestId}/run`, { method: "POST" }),
  },
  alerts: {
    rules: () => fetchApi<AlertRuleDto[]>("/api/alert/rules"),
    createRule: (rule: { name: string; conditionType: string; threshold: number; severity: string; sendEmail: boolean; sendSms: boolean }) =>
      fetchApi<AlertRuleDto>("/api/alert/rules", { method: "POST", body: JSON.stringify(rule) }),
    deleteRule: (id: string) => fetchApi<void>(`/api/alert/rules/${id}`, { method: "DELETE" }),
    toggleRule: (id: string) => fetchApi<void>(`/api/alert/rules/${id}/toggle`, { method: "PUT" }),
    history: () => fetchApi<AlertEventDto[]>("/api/alert/history"),
    acknowledge: (id: string) => fetchApi<void>(`/api/alert/history/${id}/acknowledge`, { method: "PUT" }),
  },
  strategies: {
    list: () => fetchApi<StrategyDto[]>("/api/strategy"),
    get: (id: string) => fetchApi<StrategyDto>(`/api/strategy/${id}`),
  },
};
