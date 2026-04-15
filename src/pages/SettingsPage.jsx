import { useAppStore } from '@/store/appStore';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { toast } from 'sonner';
import { useState } from 'react';

const SettingsPage = () => {
  const { settings, updateSettings } = useAppStore();
  const [warning, setWarning] = useState(settings.warningThreshold);
  const [blacklist, setBlacklist] = useState(settings.blacklistThreshold);

  const handleSave = () => {
    updateSettings({ warningThreshold: warning, blacklistThreshold: blacklist });
    toast.success('Settings saved');
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h2 className="text-2xl font-bold">Settings</h2>
        <p className="text-sm text-muted-foreground">Configure system thresholds and preferences</p>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-sm font-semibold">Attendance Thresholds</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Warning Threshold (%)</Label>
            <Input type="number" min={0} max={100} value={warning} onChange={e => setWarning(Number(e.target.value))} />
            <p className="text-xs text-muted-foreground">Students below this % will receive a warning badge</p>
          </div>
          <div className="space-y-2">
            <Label>Blacklist Threshold (%)</Label>
            <Input type="number" min={0} max={100} value={blacklist} onChange={e => setBlacklist(Number(e.target.value))} />
            <p className="text-xs text-muted-foreground">Students below this % will be added to the blacklist</p>
          </div>
          <Separator />
          <Button onClick={handleSave}>Save Settings</Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle className="text-sm font-semibold">Role Permissions</CardTitle></CardHeader>
        <CardContent>
          <div className="space-y-3">
            {[
              { role: 'Admin', perms: 'Full access to all features, settings, and reports' },
              { role: 'Faculty', perms: 'Mark attendance, view students, view reports' },
              { role: 'Student', perms: 'View own attendance, notifications' },
            ].map(r => (
              <div key={r.role} className="flex items-center justify-between p-3 rounded-lg bg-muted/50">
                <div>
                  <p className="text-sm font-medium">{r.role}</p>
                  <p className="text-xs text-muted-foreground">{r.perms}</p>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default SettingsPage;
