import { useState } from 'react';
import { mockStudents } from '@/services/mockData';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { AlertTriangle, Search } from 'lucide-react';
import { StatusBadge } from '@/components/StatusBadge';

const BlacklistPage = () => {
  const [classFilter, setClassFilter] = useState('all');
  const [search, setSearch] = useState('');

  const atRisk = mockStudents
    .filter(s => s.attendance < 75)
    .filter(s => classFilter === 'all' || s.classId === classFilter)
    .filter(s => s.name.toLowerCase().includes(search.toLowerCase()))
    .sort((a, b) => a.attendance - b.attendance);

  const blacklisted = atRisk.filter(s => s.attendance < 50);
  const warnings = atRisk.filter(s => s.attendance >= 50 && s.attendance < 75);

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold">Blacklist & Warnings</h2>
        <p className="text-sm text-muted-foreground">Students with low attendance</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-lg">
        <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4">
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <span className="text-sm font-semibold text-destructive">Blacklisted</span>
          </div>
          <p className="text-2xl font-bold mt-1">{blacklisted.length}</p>
          <p className="text-xs text-muted-foreground">Below 50% attendance</p>
        </div>
        <div className="rounded-lg border border-warning/30 bg-warning/5 p-4">
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-warning" />
            <span className="text-sm font-semibold text-warning">Warnings</span>
          </div>
          <p className="text-2xl font-bold mt-1">{warnings.length}</p>
          <p className="text-xs text-muted-foreground">Below 75% attendance</p>
        </div>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input placeholder="Search students..." className="pl-9" value={search} onChange={e => setSearch(e.target.value)} />
            </div>
            <Select value={classFilter} onValueChange={setClassFilter}>
              <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                <SelectItem value="c1">BCA Sem 4</SelectItem>
                <SelectItem value="c2">MCA Sem 2</SelectItem>
                <SelectItem value="c3">BCA Sem 6</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Roll No</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Attendance</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {atRisk.map(s => (
                <TableRow key={s.id}>
                  <TableCell className="font-mono text-xs">{s.rollNo}</TableCell>
                  <TableCell className="font-medium">{s.name}</TableCell>
                  <TableCell className="text-muted-foreground">{s.className}</TableCell>
                  <TableCell className={`font-bold ${s.attendance < 50 ? 'text-destructive' : 'text-warning'}`}>{s.attendance}%</TableCell>
                  <TableCell><StatusBadge status={s.status} /></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
};

export default BlacklistPage;
