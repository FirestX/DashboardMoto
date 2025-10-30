'use client';

import React, { useState, useEffect } from 'react';
// We assume these components are available in the runtime environment (like a Canvas or Next.js app)
import { BarChart } from '@mui/x-charts/BarChart';
import Box from '@mui/material/Box';
import { Skeleton } from '@mui/material'; // Adding Skeleton for a nice loading state

// --- Data Definition ---
// Secondary Axis (Right) - Users Visited (UV)
const uData = [4000, 3000, 2000, 2780, 1890, 2390, 3490]; 
// Primary Axis (Left) - Page Views (PV)
const pData = [2400, 1398, 9800, 3908, 4800, 3800, 4300]; 

const xLabels = [
  'Page A',
  'Page B',
  'Page C',
  'Page D',
  'Page E',
  'Page F',
  'Page G',
];

/**
 * Renders a Biaxial Bar Chart using @mui/x-charts.
 * This component includes a hydration fix to prevent errors in server-rendered environments
 * by ensuring the BarChart only renders on the client side after mounting.
 */
function BiaxialBarChart() {
  // State to track if the component has mounted on the client
  const [isClient, setIsClient] = useState(false);

  // Set isClient to true once the component has mounted on the client
  useEffect(() => {
    setIsClient(true);
  }, []);

  const chartHeight = 300;

  return (
    // Use Box for MUI styling container (this part is safe to render server-side)
    <Box 
      sx={{ 
        width: '100%', 
        height: chartHeight + 100, // Container height
        p: 4, 
        bgcolor: 'white',
        borderRadius: '12px',
        boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
      }}
      className="max-w-4xl mx-auto"
    >
      <h2 className="text-2xl font-bold mb-6 text-gray-800 border-b pb-2">Website Traffic & Unique Users</h2>
      
      {/* CRITICAL FIX: Only render the BarChart component if we are running on the client (isClient is true).
        If not, we render a simple placeholder.
      */}
      {isClient ? (
        <BarChart
          height={chartHeight} // Set inner chart height
          series={[
            {
              data: pData,
              label: 'Page Views (PV)',
              id: 'pvId',
              yAxisId: 'leftAxisId',
              color: '#3b82f6', // Blue
            },
            {
              data: uData,
              label: 'Unique Users (UV)',
              id: 'uvId',
              yAxisId: 'rightAxisId',
              color: '#f97316', // Orange
            },
          ]}
          xAxis={[{ data: xLabels, scaleType: 'band', label: 'Pages' }]}
          yAxis={[
            // Left Axis (Primary)
            { 
              id: 'leftAxisId', 
              width: 70, 
              label: 'Page Views',
            },
            // Right Axis (Secondary)
            { 
              id: 'rightAxisId', 
              position: 'right', 
              width: 70, 
              label: 'Unique Users',
            },
          ]}
          // Adjust margins to accommodate dual axes and labels
          margin={{ top: 40, right: 80, bottom: 40, left: 70 }}
        />
      ) : (
        // Placeholder/Loading State for server-side render
        <Box sx={{ height: chartHeight, display: 'flex', flexDirection: 'column', gap: 1, p: 2 }}>
            <Skeleton variant="text" sx={{ fontSize: '1rem', width: '60%' }} />
            <Skeleton variant="rectangular" height={chartHeight - 40} />
        </Box>
      )}
    </Box>
  );
}

// The main application component that sets up the environment and renders the chart.
const App = () => {
  return (
    <div className="min-h-screen bg-gray-50 p-8 flex items-start justify-center font-sans">
      <div className="w-full max-w-4xl">
        <BiaxialBarChart />
      </div>
    </div>
  );
};

export default App;