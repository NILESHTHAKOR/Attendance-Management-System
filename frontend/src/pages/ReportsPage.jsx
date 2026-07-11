import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, LineChart, Line } from 'recharts';
import { Download, FileText } from 'lucide-react';
import { mockClasses, mockStudents, monthlyAttendanceData, subjectWiseAttendance } from '@/services/mockData';
import { toast } from 'sonner';

const COLORS = ['hsl(220,70%,50%)', 'hsl(160,60%,45%)', 'hsl(38,92%,50%)', 'hsl(280,65%,60%)'];

const classData = mockClasses.map(c => ({
  name: `${c.name} S${c.semester}`,
  attendance: 65 + Math.floor(Math.random() * 30),
  students: c.studentCount,
}));

const ReportsPage = () => (
  <div className="space-y-6">
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 className="text-2xl font-bold">Reports & Analytics</h2>
        <p className="text-sm text-muted-foreground">Generate and export attendance reports</p>
      </div>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" onClick={() => toast.info('PDF export coming soon')}><FileText className="mr-2 h-4 w-4" />Export PDF</Button>
        <Button variant="outline" size="sm" onClick={() => toast.info('Excel export coming soon')}><Download className="mr-2 h-4 w-4" />Export Excel</Button>
      </div>
    </div>

    <Tabs defaultValue="class">
      <TabsList>
        <TabsTrigger value="class">Class-wise</TabsTrigger>
        <TabsTrigger value="student">Student-wise</TabsTrigger>
        <TabsTrigger value="subject">Subject-wise</TabsTrigger>
      </TabsList>

      <TabsContent value="class" className="mt-4 space-y-4">
        <Card>
          <CardHeader><CardTitle className="text-sm font-semibold">Class Attendance Comparison</CardTitle></CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={classData}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="name" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <YAxis tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <Tooltip contentStyle={{ backgroundColor: 'hsl(var(--card))', border: '1px solid hsl(var(--border))', borderRadius: '8px' }} />
                <Bar dataKey="attendance" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </TabsContent>

      <TabsContent value="student" className="mt-4 space-y-4">
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-semibold">Student Attendance Distribution</CardTitle>
              <Select defaultValue="c1">
                <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
                <SelectContent>
                  {mockClasses.map(c => <SelectItem key={c.id} value={c.id}>{c.name} Sem {c.semester}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={mockStudents.slice(0, 8).map(s => ({ name: s.name.split(' ')[0], attendance: s.attendance }))}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="name" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <YAxis tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <Tooltip contentStyle={{ backgroundColor: 'hsl(var(--card))', border: '1px solid hsl(var(--border))', borderRadius: '8px' }} />
                <Bar dataKey="attendance" fill="hsl(var(--chart-2))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </TabsContent>

      <TabsContent value="subject" className="mt-4 space-y-4">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Card>
            <CardHeader><CardTitle className="text-sm font-semibold">Subject Attendance</CardTitle></CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie data={subjectWiseAttendance} cx="50%" cy="50%" outerRadius={90} dataKey="attendance" label={({ subject, attendance }) => `${attendance}%`}>
                    {subjectWiseAttendance.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
          <Card>
            <CardHeader><CardTitle className="text-sm font-semibold">Trend by Subject</CardTitle></CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={monthlyAttendanceData}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="month" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                  <YAxis tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                  <Tooltip contentStyle={{ backgroundColor: 'hsl(var(--card))', border: '1px solid hsl(var(--border))', borderRadius: '8px' }} />
                  <Line type="monotone" dataKey="attendance" stroke="hsl(var(--chart-1))" strokeWidth={2} />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </div>
      </TabsContent>
    </Tabs>
  </div>
);

export default ReportsPage;
