import { useState, useCallback } from 'react';
import { mockStudents, mockClasses, mockSubjects } from '@/services/mockData';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Check, X, Clock, CheckCheck, Save } from 'lucide-react';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';
import { Input } from '@/components/ui/input';

const AttendancePage = () => {
  const [selectedClass, setSelectedClass] = useState('c1');
  const [selectedSubject, setSelectedSubject] = useState('sub1');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const classStudents = mockStudents.filter(s => s.classId === selectedClass);

  const [attendance, setAttendance] = useState(() =>
    Object.fromEntries(classStudents.map(s => [s.id, 'present']))
  );

  const setStatus = useCallback((id, status) => {
    setAttendance(prev => ({ ...prev, [id]: status }));
  }, []);

  const markAllPresent = () => {
    setAttendance(Object.fromEntries(classStudents.map(s => [s.id, 'present'])));
    toast.success('All marked present');
  };

  const saveAttendance = () => {
    toast.success(`Attendance saved for ${classStudents.length} students`);
  };

  const subjects = mockSubjects.filter(s => s.classId === selectedClass);
  const presentCount = Object.values(attendance).filter(s => s === 'present').length;
  const absentCount = Object.values(attendance).filter(s => s === 'absent').length;
  const lateCount = Object.values(attendance).filter(s => s === 'late').length;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold">Mark Attendance</h2>
          <p className="text-sm text-muted-foreground">Select class and date to mark attendance</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={markAllPresent}><CheckCheck className="mr-2 h-4 w-4" />All Present</Button>
          <Button size="sm" onClick={saveAttendance}><Save className="mr-2 h-4 w-4" />Save</Button>
        </div>
      </div>

      <div className="flex flex-wrap gap-3">
        <Select value={selectedClass} onValueChange={setSelectedClass}>
          <SelectTrigger className="w-44"><SelectValue /></SelectTrigger>
          <SelectContent>
            {mockClasses.map(c => <SelectItem key={c.id} value={c.id}>{c.name} Sem {c.semester}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={selectedSubject} onValueChange={setSelectedSubject}>
          <SelectTrigger className="w-44"><SelectValue /></SelectTrigger>
          <SelectContent>
            {subjects.map(s => <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Input type="date" value={date} onChange={e => setDate(e.target.value)} className="w-44" />
      </div>

      <div className="grid grid-cols-3 gap-3 max-w-md">
        <div className="rounded-lg bg-success/10 p-3 text-center">
          <p className="text-lg font-bold text-success">{presentCount}</p>
          <p className="text-xs text-muted-foreground">Present</p>
        </div>
        <div className="rounded-lg bg-destructive/10 p-3 text-center">
          <p className="text-lg font-bold text-destructive">{absentCount}</p>
          <p className="text-xs text-muted-foreground">Absent</p>
        </div>
        <div className="rounded-lg bg-warning/10 p-3 text-center">
          <p className="text-lg font-bold text-warning">{lateCount}</p>
          <p className="text-xs text-muted-foreground">Late</p>
        </div>
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-24">Roll No</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="hidden sm:table-cell">Attendance %</TableHead>
                  <TableHead className="text-center">Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {classStudents.map(s => (
                  <TableRow key={s.id}>
                    <TableCell className="font-mono text-xs">{s.rollNo}</TableCell>
                    <TableCell className="font-medium">{s.name}</TableCell>
                    <TableCell className="hidden sm:table-cell text-muted-foreground">{s.attendance}%</TableCell>
                    <TableCell>
                      <div className="flex gap-1.5 justify-center">
                        {[
                          ['present', Check, 'bg-success text-success-foreground'],
                          ['absent', X, 'bg-destructive text-destructive-foreground'],
                          ['late', Clock, 'bg-warning text-warning-foreground'],
                        ].map(([status, Icon, activeClass]) => (
                          <button
                            key={status}
                            onClick={() => setStatus(s.id, status)}
                            className={cn(
                              'rounded-lg p-2 transition-all',
                              attendance[s.id] === status ? activeClass : 'bg-muted text-muted-foreground hover:bg-muted/80'
                            )}
                          >
                            <Icon className="h-4 w-4" />
                          </button>
                        ))}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default AttendancePage;
