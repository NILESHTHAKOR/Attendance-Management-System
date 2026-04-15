import { StatCard } from '@/components/StatCard';
import { BookOpen, CalendarCheck, AlertTriangle, Clock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/StatusBadge';
import { mockStudents, mockClasses } from '@/services/mockData';
import { useNavigate } from 'react-router-dom';

const FacultyDashboard = () => {
  const navigate = useNavigate();
  const lowAttendance = mockStudents.filter(s => s.attendance < 75);
  const todayClasses = mockClasses.slice(0, 3);

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold">Faculty Dashboard</h2>
        <p className="text-sm text-muted-foreground">Welcome back, Prof. Anita Sharma</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Today's Classes" value={3} icon={BookOpen} subtitle="2 completed, 1 upcoming" />
        <StatCard title="Pending Attendance" value={1} icon={CalendarCheck} subtitle="BCA Sem 4 - Section A" variant="warning" />
        <StatCard title="Low Attendance" value={lowAttendance.length} icon={AlertTriangle} subtitle="Students below 75%" variant="warning" />
        <StatCard title="Next Class" value="2:00 PM" icon={Clock} subtitle="Data Structures - BCA Sem 4" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-sm font-semibold">Today's Schedule</CardTitle>
            <Button variant="outline" size="sm" onClick={() => navigate('/attendance')}>Mark Attendance</Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {todayClasses.map(c => (
              <div key={c.id} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                <div>
                  <p className="text-sm font-medium">{c.name} Sem {c.semester} - Sec {c.section}</p>
                  <p className="text-xs text-muted-foreground">{c.department}</p>
                </div>
                <Button size="sm" onClick={() => navigate('/attendance')}>Mark</Button>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-sm font-semibold">Low Attendance Alerts</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            {lowAttendance.slice(0, 5).map(s => (
              <div key={s.id} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                <div>
                  <p className="text-sm font-medium">{s.name}</p>
                  <p className="text-xs text-muted-foreground">{s.rollNo} • {s.className}</p>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-bold">{s.attendance}%</span>
                  <StatusBadge status={s.status} />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default FacultyDashboard;
