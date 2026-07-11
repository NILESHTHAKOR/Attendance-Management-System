import { subjectWiseAttendance, monthlyAttendanceData, heatmapData } from '@/services/mockData';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Progress } from '@/components/ui/progress';
import { cn } from '@/lib/utils';

const MyAttendancePage = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-2xl font-bold">My Attendance</h2>
      <p className="text-sm text-muted-foreground">Detailed view of your attendance records</p>
    </div>

    <Card>
      <CardHeader><CardTitle className="text-sm font-semibold">Attendance Heatmap - 2026</CardTitle></CardHeader>
      <CardContent>
        <div className="flex gap-1 flex-wrap">
          {Object.entries(heatmapData).map(([date, val]) => (
            <div
              key={date}
              title={`${date}: ${val}%`}
              className={cn(
                'h-3 w-3 rounded-sm',
                val >= 90 ? 'bg-success' : val >= 75 ? 'bg-success/60' : val >= 50 ? 'bg-warning/60' : 'bg-destructive/60'
              )}
            />
          ))}
        </div>
        <div className="flex gap-3 mt-3 text-xs text-muted-foreground">
          <span className="flex items-center gap-1"><div className="h-2.5 w-2.5 rounded-sm bg-destructive/60" />{'<50%'}</span>
          <span className="flex items-center gap-1"><div className="h-2.5 w-2.5 rounded-sm bg-warning/60" />50-74%</span>
          <span className="flex items-center gap-1"><div className="h-2.5 w-2.5 rounded-sm bg-success/60" />75-89%</span>
          <span className="flex items-center gap-1"><div className="h-2.5 w-2.5 rounded-sm bg-success" />90%+</span>
        </div>
      </CardContent>
    </Card>

    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <Card>
        <CardHeader><CardTitle className="text-sm font-semibold">Subject Breakdown</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          {subjectWiseAttendance.map(s => (
            <div key={s.subject} className="space-y-1.5">
              <div className="flex justify-between text-sm">
                <span className="font-medium">{s.subject}</span>
                <span className={`font-bold ${s.attendance < 50 ? 'text-destructive' : s.attendance < 75 ? 'text-warning' : 'text-success'}`}>{s.attendance}%</span>
              </div>
              <Progress value={s.attendance} className="h-2" />
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

export default MyAttendancePage;
