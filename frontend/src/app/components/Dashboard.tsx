import { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router";
import { 
  Droplets, 
  TrendingDown, 
  TrendingUp, 
  AlertTriangle, 
  Award,
  Calendar,
  Plus,
  LogOut,
  BookOpen,
  BarChart3,
  Bell,
  Check
} from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Alert, AlertDescription } from "./ui/alert";
import { Badge } from "./ui/badge";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { fetchApi } from "../../lib/api";

export default function Dashboard() {
  const navigate = useNavigate();
  const [selectedPeriod, setSelectedPeriod] = useState<"daily" | "weekly" | "monthly">("weekly");
  const [data, setData] = useState<any>(null);
  const [notifications, setNotifications] = useState<any[]>([]);
  
  const userName = localStorage.getItem("userName") || "Usuário";

  useEffect(() => {
    const loadData = async () => {
      try {
        const summary = await fetchApi("/dashboard/summary");
        setData(summary);
        const notifs = await fetchApi("/notifications");
        setNotifications(notifs);
      } catch (err) {
        console.error(err);
      }
    };
    loadData();
  }, []);

  const markAsRead = async (id: number) => {
    try {
      await fetchApi(`/notifications/read/${id}`, { method: "POST" });
      setNotifications(notifications.map(n => n.id === id ? { ...n, isRead: true } : n));
    } catch (err) {
      console.error(err);
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    navigate("/");
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  if (!data) {
    return <div className="min-h-screen flex items-center justify-center">Carregando dados do servidor...</div>;
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <header className="bg-white border-b border-cyan-200 sticky top-0 z-10 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="bg-gradient-to-br from-cyan-500 to-emerald-500 p-2 rounded-lg">
              <Droplets className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-xl text-slate-800">AquaMonitor</h1>
              <p className="text-xs text-slate-500">Residência - {userName}</p>
            </div>
          </div>
          <div className="flex gap-4 items-center">
            {/* Notificações / Caixa de Mensagem */}
            <Popover>
              <PopoverTrigger asChild>
                <Button variant="ghost" size="icon" className="relative group hover:bg-slate-100">
                  <Bell className="w-5 h-5 text-slate-600 group-hover:text-cyan-600" />
                  {unreadCount > 0 && (
                    <span className="absolute top-0 right-0 w-4 h-4 bg-red-500 rounded-full text-[10px] text-white flex items-center justify-center font-bold">
                      {unreadCount}
                    </span>
                  )}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-80 p-0" align="end">
                <div className="p-4 border-b border-slate-100 flex items-center justify-between">
                  <h4 className="font-semibold text-slate-800">Mensagens da Caixa</h4>
                  <Badge variant="secondary" className="bg-cyan-100 text-cyan-800">{unreadCount} novas</Badge>
                </div>
                <div className="max-h-80 overflow-y-auto">
                  {notifications.length === 0 ? (
                    <p className="p-4 text-sm text-slate-500 text-center">Nenhuma mensagem no momento.</p>
                  ) : (
                    notifications.map(n => (
                      <div key={n.id} className={`p-4 border-b border-slate-50 last:border-0 ${!n.isRead ? 'bg-cyan-50/50' : 'bg-white'}`}>
                        <div className="flex justify-between items-start mb-1">
                          <h5 className={`text-sm ${!n.isRead ? 'font-semibold text-cyan-900' : 'font-medium text-slate-700'}`}>
                            {n.title}
                          </h5>
                          {!n.isRead && (
                            <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => markAsRead(n.id)}>
                              <Check className="w-4 h-4 text-emerald-500" />
                            </Button>
                          )}
                        </div>
                        <p className="text-xs text-slate-600">{n.message}</p>
                        <p className="text-[10px] text-slate-400 mt-2">{new Date(n.createdAt).toLocaleString()}</p>
                      </div>
                    ))
                  )}
                </div>
              </PopoverContent>
            </Popover>

            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate("/tips")}
              className="text-emerald-700 hover:bg-emerald-50"
            >
              <BookOpen className="w-4 h-4 mr-2" />
              Dicas
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={logout}
              className="text-slate-600 hover:bg-slate-100"
            >
              <LogOut className="w-4 h-4 mr-2" />
              Sair
            </Button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 py-6 space-y-6">
        {/* Alertas */}
        <Alert className="border-amber-500 bg-amber-50">
          <AlertTriangle className="w-5 h-5 text-amber-600" />
          <AlertDescription className="text-amber-800 ml-2">
            <strong>Atenção!</strong> Seu consumo recente tem tido picos acima da média. 
            Confira sua Caixa de Mensagens ou as <Link to="/tips" className="underline font-semibold">dicas de economia</Link>.
          </AlertDescription>
        </Alert>

        {/* Cards de Resumo */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card className="border-cyan-200 bg-gradient-to-br from-cyan-50 to-white shadow-sm">
            <CardHeader className="pb-2">
              <CardDescription>Consumo Hoje</CardDescription>
              <CardTitle className="text-3xl text-cyan-700">{data.currentConsumption}L</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm">
                {data.currentConsumption > data.averageConsumption ? (
                  <>
                    <TrendingUp className="w-4 h-4 text-red-500" />
                    <span className="text-red-600">+6% acima da média</span>
                  </>
                ) : (
                  <>
                    <TrendingDown className="w-4 h-4 text-green-500" />
                    <span className="text-green-600">Abaixo da média</span>
                  </>
                )}
              </div>
            </CardContent>
          </Card>

          <Card className="border-blue-200 bg-gradient-to-br from-blue-50 to-white shadow-sm">
            <CardHeader className="pb-2">
              <CardDescription>Média Diária</CardDescription>
              <CardTitle className="text-3xl text-blue-700">{data.averageConsumption}L</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <Calendar className="w-4 h-4" />
                <span>Últimos 7 dias</span>
              </div>
            </CardContent>
          </Card>

          <Card className="border-emerald-200 bg-gradient-to-br from-emerald-50 to-white shadow-sm">
            <CardHeader className="pb-2">
              <CardDescription>Total do Mês</CardDescription>
              <CardTitle className="text-3xl text-emerald-700">{data.monthTotal}L</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm">
                <TrendingDown className="w-4 h-4 text-emerald-500" />
                <span className="text-emerald-600">{data.savings}% abaixo da meta</span>
              </div>
            </CardContent>
          </Card>

          <Card className="border-purple-200 bg-gradient-to-br from-purple-50 to-white shadow-sm">
            <CardHeader className="pb-2">
              <CardDescription>Economia Estimada</CardDescription>
              <CardTitle className="text-3xl text-purple-700">R$ 18,50</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm text-slate-600">
                <Award className="w-4 h-4 text-purple-500" />
                <span>Neste mês</span>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Botão de Registro Rápido */}
        <Card className="border-2 border-dashed border-cyan-300 bg-cyan-50/50 hover:bg-cyan-100/50 transition-colors shadow-sm cursor-pointer">
          <CardContent className="p-6" onClick={() => navigate("/data-entry")}>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <div className="bg-cyan-500 p-3 rounded-full shadow">
                  <Plus className="w-6 h-6 text-white" />
                </div>
                <div>
                  <h3 className="text-lg text-slate-800">Registrar Consumo</h3>
                  <p className="text-sm text-slate-600">Adicione sua leitura de hoje</p>
                </div>
              </div>
              <Button className="bg-cyan-600 hover:bg-cyan-700 text-white shadow">
                <BarChart3 className="w-4 h-4 mr-2" />
                Adicionar Dados
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Seletor de Período */}
        <div className="flex gap-2">
          <Button
            variant={selectedPeriod === "daily" ? "default" : "outline"}
            onClick={() => setSelectedPeriod("daily")}
            className={selectedPeriod === "daily" ? "bg-cyan-600 hover:bg-cyan-700 shadow" : "border-cyan-300 text-cyan-700 hover:bg-cyan-50"}
          >
            Hoje
          </Button>
          <Button
            variant={selectedPeriod === "weekly" ? "default" : "outline"}
            onClick={() => setSelectedPeriod("weekly")}
            className={selectedPeriod === "weekly" ? "bg-cyan-600 hover:bg-cyan-700 shadow" : "border-cyan-300 text-cyan-700 hover:bg-cyan-50"}
          >
            Semana
          </Button>
          <Button
            variant={selectedPeriod === "monthly" ? "default" : "outline"}
            onClick={() => setSelectedPeriod("monthly")}
            className={selectedPeriod === "monthly" ? "bg-cyan-600 hover:bg-cyan-700 shadow" : "border-cyan-300 text-cyan-700 hover:bg-cyan-50"}
          >
            Mês
          </Button>
        </div>

        {/* Gráficos */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Gráfico Principal */}
          <Card className="border-cyan-200 col-span-full shadow-sm">
            <CardHeader>
              <CardTitle className="text-slate-800">
                {selectedPeriod === "daily" && "Consumo por Hora - Hoje"}
                {selectedPeriod === "weekly" && "Consumo Semanal"}
                {selectedPeriod === "monthly" && "Consumo Mensal"}
              </CardTitle>
              <CardDescription>
                {selectedPeriod === "daily" && "Litros consumidos por período do dia"}
                {selectedPeriod === "weekly" && "Comparativo com a média diária"}
                {selectedPeriod === "monthly" && "Comparativo com a meta mensal"}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={350}>
                {selectedPeriod === "daily" && (
                  <AreaChart data={data.dailyData}>
                    <defs>
                      <linearGradient id="colorConsumption" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#0891B2" stopOpacity={0.8}/>
                        <stop offset="95%" stopColor="#0891B2" stopOpacity={0}/>
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                    <XAxis dataKey="time" stroke="#64748b" />
                    <YAxis stroke="#64748b" label={{ value: 'Litros', angle: -90, position: 'insideLeft' }} />
                    <Tooltip 
                      contentStyle={{ background: '#fff', border: '1px solid #0891B2', borderRadius: '8px' }}
                      formatter={(value: any) => [`${value}L`, 'Consumo']}
                    />
                    <Area 
                      type="monotone" 
                      dataKey="consumption" 
                      stroke="#0891B2" 
                      strokeWidth={2}
                      fillOpacity={1} 
                      fill="url(#colorConsumption)" 
                    />
                  </AreaChart>
                )}
                {selectedPeriod === "weekly" && (
                  <BarChart data={data.weeklyData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                    <XAxis dataKey="day" stroke="#64748b" />
                    <YAxis stroke="#64748b" label={{ value: 'Litros', angle: -90, position: 'insideLeft' }} />
                    <Tooltip 
                      contentStyle={{ background: '#fff', border: '1px solid #0891B2', borderRadius: '8px' }}
                      formatter={(value: any) => [`${value}L`]}
                    />
                    <Legend />
                    <Bar dataKey="consumption" fill="#0891B2" name="Consumo" radius={[8, 8, 0, 0]} />
                    <Bar dataKey="average" fill="#10B981" name="Média" radius={[8, 8, 0, 0]} />
                  </BarChart>
                )}
                {selectedPeriod === "monthly" && (
                  <LineChart data={data.monthlyData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                    <XAxis dataKey="month" stroke="#64748b" />
                    <YAxis stroke="#64748b" label={{ value: 'Litros', angle: -90, position: 'insideLeft' }} />
                    <Tooltip 
                      contentStyle={{ background: '#fff', border: '1px solid #0891B2', borderRadius: '8px' }}
                      formatter={(value: any) => [`${value}L`]}
                    />
                    <Legend />
                    <Line 
                      type="monotone" 
                      dataKey="consumption" 
                      stroke="#0891B2" 
                      strokeWidth={3}
                      name="Consumo"
                      dot={{ fill: '#0891B2', r: 6 }}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="goal" 
                      stroke="#10B981" 
                      strokeWidth={2}
                      strokeDasharray="5 5"
                      name="Meta"
                      dot={{ fill: '#10B981', r: 4 }}
                    />
                  </LineChart>
                )}
              </ResponsiveContainer>
            </CardContent>
          </Card>

          {/* Card de Conquistas */}
          <Card className="border-emerald-200 bg-gradient-to-br from-emerald-50 to-white shadow-sm">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-emerald-800">
                <Award className="w-5 h-5" />
                Conquistas
              </CardTitle>
              <CardDescription>Suas metas alcançadas</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-emerald-200">
                <div className="flex items-center gap-3">
                  <div className="bg-emerald-100 p-2 rounded-full">
                    <TrendingDown className="w-5 h-5 text-emerald-600" />
                  </div>
                  <div>
                    <p className="text-sm font-semibold text-slate-700">7 dias de economia</p>
                    <p className="text-xs text-slate-500">Consecutivos abaixo da meta</p>
                  </div>
                </div>
                <Badge className="bg-emerald-500">Ativo</Badge>
              </div>
              <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-emerald-200">
                <div className="flex items-center gap-3">
                  <div className="bg-blue-100 p-2 rounded-full">
                    <Droplets className="w-5 h-5 text-blue-600" />
                  </div>
                  <div>
                    <p className="text-sm font-semibold text-slate-700">500L economizados</p>
                    <p className="text-xs text-slate-500">Neste mês</p>
                  </div>
                </div>
                <Badge className="bg-blue-500">Novo</Badge>
              </div>
              <div className="flex items-center justify-between p-3 bg-white rounded-lg border border-slate-200 opacity-60">
                <div className="flex items-center gap-3">
                  <div className="bg-slate-100 p-2 rounded-full">
                    <Award className="w-5 h-5 text-slate-400" />
                  </div>
                  <div>
                    <p className="text-sm font-semibold text-slate-500">Eco Warrior</p>
                    <p className="text-xs text-slate-400">Economize 1000L em um mês</p>
                  </div>
                </div>
                <Badge variant="outline">Bloqueado</Badge>
              </div>
            </CardContent>
          </Card>

          {/* Card de Comparação */}
          <Card className="border-blue-200 bg-gradient-to-br from-blue-50 to-white shadow-sm">
            <CardHeader>
              <CardTitle className="text-blue-800">Comparativo</CardTitle>
              <CardDescription>Você vs. outras residências</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-slate-600">Você (3 pessoas)</span>
                  <span className="font-semibold text-cyan-700">{data.averageConsumption}L/dia</span>
                </div>
                <div className="h-3 bg-slate-200 rounded-full overflow-hidden">
                  <div className="h-full bg-gradient-to-r from-cyan-500 to-cyan-400" style={{ width: '68%' }}></div>
                </div>
              </div>
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-slate-600">Média Regional</span>
                  <span className="font-semibold text-slate-700">245L/dia</span>
                </div>
                <div className="h-3 bg-slate-200 rounded-full overflow-hidden">
                  <div className="h-full bg-gradient-to-r from-slate-400 to-slate-300" style={{ width: '76%' }}></div>
                </div>
              </div>
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-slate-600">Média Nacional</span>
                  <span className="font-semibold text-slate-700">280L/dia</span>
                </div>
                <div className="h-3 bg-slate-200 rounded-full overflow-hidden">
                  <div className="h-full bg-gradient-to-r from-slate-500 to-slate-400" style={{ width: '87%' }}></div>
                </div>
              </div>
              <div className="mt-4 p-3 bg-emerald-100 rounded-lg">
                <p className="text-sm text-emerald-800 text-center font-semibold">
                  🎉 Você está consumindo 21% menos que a média nacional!
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  );
}
