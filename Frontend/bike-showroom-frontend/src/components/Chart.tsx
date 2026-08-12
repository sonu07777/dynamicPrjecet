import { useEffect, useRef } from 'react';
import * as echarts from 'echarts';

interface ChartProps {
  option: echarts.EChartsOption;
  style?: React.CSSProperties;
  className?: string;
  height?: number;
}

const Chart = ({ option, style, className, height = 300 }: ChartProps) => {
  const chartRef = useRef<HTMLDivElement>(null);
  const instanceRef = useRef<echarts.ECharts | null>(null);

  useEffect(() => {
    if (!chartRef.current) return;
    if (!instanceRef.current) {
      instanceRef.current = echarts.init(chartRef.current, undefined, { renderer: 'canvas' });
    }
    instanceRef.current.setOption(option, true);
    const handleResize = () => instanceRef.current?.resize();
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
      instanceRef.current?.dispose();
      instanceRef.current = null;
    };
  }, [option]);

  return (
    <div
      ref={chartRef}
      className={className}
      style={{ width: '100%', height, ...style }}
    />
  );
};

export default Chart;
