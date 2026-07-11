import { Card, CardContent } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';

const logs = [
  { id: '1', action: 'Attendance Marked', user: 'Prof. Anita Sharma', target: 'BCA Sem 4 - CS301', time: '2026-04-13 10:30 AM', type: 'attendance' },
  { id: '2', action: 'Student Added', user: 'Dr. Rajesh Kumar', target: 'Rahul Verma (BCA-001)', time: '2026-04-12 03:15 PM', type: 'student' },
  { id: '3', action: 'Settings Updated', user: 'Dr. Rajesh Kumar', target: 'Warning threshold: 75%', time: '2026-04-12 02:00 PM', type: 'settings' },
  { id: '4', action: 'Blacklist Generated', user: 'System', target: '3 students flagged', time: '2026-04-11 11:00 PM', type: 'system' },
  { id: '5', action: 'Attendance Edited', user: 'Prof. Anita Sharma', target: 'MCA Sem 2 - MCA201', time: '2026-04-11 04:45 PM', type: 'attendance' },
  { id: '6', action: 'Student Removed', user: 'Dr. Rajesh Kumar', target: 'Test Student', time: '2026-04-10 09:00 AM', type: 'student' },
];

const typeColors = {
  attendance: 'bg-primary/10 text-primary',
  student: 'bg-success/10 text-success',
  settings: 'bg-warning/10 text-warning',
  system: 'bg-muted text-muted-foreground',
};

const AuditLogsPage = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-2xl font-bold">Audit Logs</h2>
      <p className="text-sm text-muted-foreground">System activity history</p>
    </div>
    <Card>
      <CardContent className="pt-6">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Action</TableHead>
              <TableHead>User</TableHead>
              <TableHead className="hidden md:table-cell">Details</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Time</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {logs.map(l => (
              <TableRow key={l.id}>
                <TableCell className="font-medium">{l.action}</TableCell>
                <TableCell className="text-muted-foreground">{l.user}</TableCell>
                <TableCell className="hidden md:table-cell text-muted-foreground text-xs">{l.target}</TableCell>
                <TableCell><Badge className={typeColors[l.type]} variant="secondary">{l.type}</Badge></TableCell>
                <TableCell className="text-xs text-muted-foreground">{l.time}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  </div>
);

export default AuditLogsPage;
