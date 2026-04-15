import { cn } from '@/lib/utils';

const variantStyles = {
  default: 'border-border bg-card',
  warning: 'border-warning/30 bg-warning/5',
  danger: 'border-destructive/30 bg-destructive/5',
  success: 'border-success/30 bg-success/5',
};

const iconVariantStyles = {
  default: 'bg-primary/10 text-primary',
  warning: 'bg-warning/15 text-warning',
  danger: 'bg-destructive/15 text-destructive',
  success: 'bg-success/15 text-success',
};

export const StatCard = ({ title, value, subtitle, icon: Icon, trend, variant = 'default' }) => (
  <div className={cn('rounded-lg border p-5 animate-slide-up transition-shadow hover:shadow-md', variantStyles[variant])}>
    <div className="flex items-start justify-between">
      <div className="space-y-1">
        <p className="text-sm font-medium text-muted-foreground">{title}</p>
        <p className="text-2xl font-bold tracking-tight">{value}</p>
        {subtitle && <p className="text-xs text-muted-foreground">{subtitle}</p>}
        {trend && (
          <p className={cn('text-xs font-medium', trend.positive ? 'text-success' : 'text-destructive')}>
            {trend.positive ? '↑' : '↓'} {Math.abs(trend.value)}% from last month
          </p>
        )}
      </div>
      <div className={cn('rounded-lg p-2.5', iconVariantStyles[variant])}>
        <Icon className="h-5 w-5" />
      </div>
    </div>
  </div>
);
