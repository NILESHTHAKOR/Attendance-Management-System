import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

const styles = {
  present: 'bg-success/15 text-success border-success/30 hover:bg-success/20',
  absent: 'bg-destructive/15 text-destructive border-destructive/30 hover:bg-destructive/20',
  late: 'bg-warning/15 text-warning border-warning/30 hover:bg-warning/20',
  active: 'bg-success/15 text-success border-success/30 hover:bg-success/20',
  warning: 'bg-warning/15 text-warning border-warning/30 hover:bg-warning/20',
  blacklisted: 'bg-destructive/15 text-destructive border-destructive/30 hover:bg-destructive/20',
};

export const StatusBadge = ({ status, className }) => (
  <Badge variant="outline" className={cn('capitalize font-medium', styles[status], className)}>
    {status}
  </Badge>
);
