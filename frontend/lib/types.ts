export interface PortfolioDto {
  totalEquity: number;
  cash: number;
  buyingPower: number;
  unrealizedPnl: number;
  dailyChange: number;
  dailyChangePercent: number;
}

export interface PositionDto {
  symbol: string;
  side: string;
  qty: number;
  entryPrice: number;
  currentPrice: number;
  unrealizedPnl: number;
  pnlPercent: number;
  broker: string;
  assetClass: string;
}

export interface OrderDto {
  id: string;
  externalId: string;
  symbol: string;
  side: string;
  type: string;
  status: string;
  qty: number;
  limitPrice?: number;
  filledQty?: number;
  filledPrice?: number;
  broker: string;
  createdAt: string;
}

export interface BacktestSummaryDto {
  id: string;
  projectId: string;
  projectName?: string;
  strategyName: string;
  dateRun: string;
  sharpeRatio: number;
  maxDrawdown: number;
  totalReturn: number;
  winRate: number;
  aiScore?: number;
  isAnalyzed: boolean;
}

export interface AlgorithmSummaryDto {
  projectId: string;
  projectName: string;
  backtestCount: number;
  analyzedCount: number;
  bestSharpe?: number;
  bestTotalReturn: number;
  latestBacktest: string;
}

export interface BacktestDetailDto extends BacktestSummaryDto {
  sortinoRatio: number;
  profitFactor: number;
  calmarRatio: number;
  totalTrades: number;
  winningTrades: number;
  losingTrades: number;
  avgWin: number;
  avgLoss: number;
  dailyReturns: DailyReturnDto[];
  trades: TradeDto[];
  aiReport?: AnalysisReportDto;
}

export interface DailyReturnDto {
  date: string;
  equity: number;
  dailyPnl: number;
  dailyReturnPercent: number;
  cumulativeReturn: number;
  drawdown: number;
}

export interface TradeDto {
  symbol: string;
  entryTime: string;
  exitTime: string;
  entryPrice: number;
  exitPrice: number;
  quantity: number;
  side: string;
  profitLoss: number;
  profitLossPercent: number;
}

export interface AnalysisReportDto {
  id: string;
  backtestId: string;
  overallAssessment: string;
  strengths: string[];
  weaknesses: string[];
  overfittingRisk: string;
  deploymentReadiness: number;
  summary: string;
  criticalWarnings: string[];
  createdAt: string;
}

export interface AlertRuleDto {
  id: string;
  name: string;
  conditionType: string;
  threshold: number;
  severity: string;
  isActive: boolean;
  sendEmail: boolean;
  sendSms: boolean;
}

export interface AlertEventDto {
  id: string;
  ruleName: string;
  severity: string;
  message: string;
  currentValue: number;
  threshold: number;
  isAcknowledged: boolean;
  triggeredAt: string;
}

export interface DashboardDto {
  portfolio: PortfolioDto;
  positions: PositionDto[];
  metrics: MetricsDto;
  recentTrades: OrderDto[];
  recentAlerts: AlertEventDto[];
}

export interface MetricsDto {
  liveSharpe30d: number;
  winRate30: number;
  currentDrawdown: number;
  openPositions: number;
  todaysPnl: number;
  availableCash: number;
}

export interface StrategyDto {
  id: string;
  name: string;
  description?: string;
  assetClass: string;
  isActive: boolean;
  createdAt: string;
}

export interface RegimeDto {
  regime: string;
  startDate: string;
  endDate: string;
  returnInRegime: number;
  sharpeInRegime: number;
}
