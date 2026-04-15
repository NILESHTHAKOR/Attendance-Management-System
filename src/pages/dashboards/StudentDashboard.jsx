import { StatCard } from '@/components/StatCard';
import { CalendarCheck, BookOpen, AlertTriangle, TrendingUp } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts';
import { monthlyAttendanceData, subjectWiseAttendance } from '@/services/mockData';
import { Progress } from '@/components/ui/progress';

const StudentDashboard = () => {
  const overallAttendance = 78;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold">My Dashboard</h2>
        <p className="text-sm text-muted-foreground">BCA Sem 4 - Section A</p>
      </div>

      {overallAttendance < 75 && (
        <div className="rounded-lg border border-warning/30 bg-warning/5 p-4 flex items-center gap-3 animate-fade-in">
          <AlertTriangle className="h-5 w-5 text-warning shrink-0" />
          <div>
            <p className="text-sm font-semibold text-warning">Attendance Warning</p>
            <p className="text-xs text-muted-foreground">Your attendance is below 75%. Maintain regular attendance to avoid blacklisting.</p>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Overall Attendance" value={`${overallAttendance}%`} icon={CalendarCheck} variant={overallAttendance < 75 ? 'warning' : 'default'} />
        <StatCard title="Classes Attended" value="124/158" icon={BookOpen} subtitle="This semester" />
        <StatCard title="Subjects" value={4} icon={TrendingUp} subtitle="Currently enrolled" />
        <StatCard title="Days Absent" value={34} icon={AlertTriangle} subtitle="This semester" variant={overallAttendance < 50 ? 'danger' : 'default'} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card>
          <CardHeader><CardTitle className="text-sm font-semibold">Subject-wise Attendance</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            {subjectWiseAttendance.map(s => (
              <div key={s.subject} className="space-y-1.5">
                <div className="flex justify-between text-sm">
                  <span className="font-medium">{s.subject}</span>
                  <span className={`font-bold ${s.attendance < 75 ? 'text-warning' : s.attendance < 50 ? 'text-destructive' : 'text-success'}`}>
                    {s.attendance}%
                  </span>
                </div>
                <Progress value={s.attendance} className="h-2" />
                <p className="text-xs text-muted-foreground">{s.present}/{s.total} classes attended</p>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm font-semibold">Monthly Trend</CardTitle></CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={monthlyAttendanceData}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="month" className="text-xs" tick={{ fill: 'hsl(var(--muted-foreground))' }} />
                <YAxis domain={[60, 100]} className="text-xs" tick={{ fill: 'hsl(var(--muted-foreground))' }} />
                <Tooltip contentStyle={{ backgroundColor: 'hsl(var(--card))', border: '1px solid hsl(var(--border))', borderRadius: '8px' }} />
                <Line type="monotone" dataKey="attendance" stroke="hsl(var(--primary))" strokeWidth={2.5} dot={{ fill: 'hsl(var(--primary))', r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default StudentDashboard;
