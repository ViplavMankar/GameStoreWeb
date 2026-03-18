window.renderChart = function (labels, sessions) {

    const ctx = document.getElementById("trendingChart").getContext("2d");

    // Neon Gradient
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, "#ff00ff");
    gradient.addColorStop(1, "#2b0040");

    new Chart(ctx, {
        type: 'bar',

        data: {
            labels: labels,
            datasets: [{
                label: 'Sessions in Last 24 Hours',
                data: sessions,

                backgroundColor: gradient,
                borderColor: "#ff00ff",
                borderWidth: 2,

                borderRadius: 12,
                hoverBorderWidth: 3,
                hoverBackgroundColor: "#ff33ff"
            }]
        },

        options: {

            responsive: true,

            animation: {
                duration: 2000,
                easing: "easeOutQuart"
            },

            plugins: {
                legend: {
                    labels: {
                        color: "#e5e7eb",
                        font: {
                            size: 14
                        }
                    }
                },

                tooltip: {
                    backgroundColor: "#111827",
                    borderColor: "#ff00ff",
                    borderWidth: 1,
                    titleColor: "#ffffff",
                    bodyColor: "#9ca3af"
                }
            },

            scales: {

                x: {
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                },

                y: {
                    beginAtZero: true,
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                }
            }
        },

        plugins: [{
            id: "neonGlow",

            beforeDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.shadowColor = "#ff00ff";
                ctx.shadowBlur = 20;
            },

            afterDraw: () => {
                const ctx = document.getElementById("trendingChart").getContext("2d");
                ctx.shadowBlur = 0;
            }
        }]
    });
};
window.renderPlaytimeChart = function (labels, minutes) {

    const ctx = document.getElementById("playtimeChart").getContext("2d");

    // Neon Gradient
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, "#00ffff");
    gradient.addColorStop(1, "#001f3f");

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Playtime (minutes)',
                data: minutes,
                backgroundColor: gradient,
                borderColor: "#00ffff",
                borderWidth: 2,
                borderRadius: 12,     // rounded bars
                hoverBorderWidth: 3
            }]
        },
        options: {

            responsive: true,

            animation: {
                duration: 2000,
                easing: 'easeOutQuart'
            },

            plugins: {
                legend: {
                    labels: {
                        color: "#e5e7eb",
                        font: {
                            size: 14
                        }
                    }
                },

                tooltip: {
                    backgroundColor: "#111827",
                    borderColor: "#00ffff",
                    borderWidth: 1,
                    titleColor: "#ffffff",
                    bodyColor: "#9ca3af"
                }
            },

            scales: {

                x: {
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                },

                y: {
                    beginAtZero: true,
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                }
            }
        },

        plugins: [{
            id: "neonGlow",

            beforeDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.shadowColor = "#00ffff";
                ctx.shadowBlur = 20;
            },

            afterDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.shadowBlur = 0;
            }
        }]
    });
};
window.renderPlayerGrowthChart = function (labels, players) {

    const ctx = document.getElementById("playerGrowthChart").getContext("2d");

    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, "rgba(0,255,120,0.6)");
    gradient.addColorStop(1, "rgba(0,255,120,0)");

    new Chart(ctx, {
        type: 'line',

        data: {
            labels: labels,
            datasets: [{
                label: "Daily Active Players",
                data: players,

                borderColor: "#00ff78",
                backgroundColor: gradient,

                borderWidth: 3,
                tension: 0.4,
                fill: true,

                pointBackgroundColor: "#00ff78",
                pointBorderColor: "#00ff78",
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },

        options: {

            responsive: true,

            animation: {
                duration: 2000,
                easing: "easeOutQuart"
            },

            plugins: {
                legend: {
                    labels: {
                        color: "#e5e7eb"
                    }
                },

                tooltip: {
                    backgroundColor: "#111827",
                    borderColor: "#00ff78",
                    borderWidth: 1,
                    titleColor: "#ffffff",
                    bodyColor: "#9ca3af"
                }
            },

            scales: {

                x: {
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                },

                y: {
                    beginAtZero: true,
                    ticks: {
                        color: "#9ca3af"
                    },
                    grid: {
                        color: "rgba(255,255,255,0.05)"
                    }
                }
            }
        },

        plugins: [{
            id: "neonGlow",

            beforeDraw: (chart) => {
                const ctx = chart.ctx;
                ctx.shadowColor = "#00ff78";
                ctx.shadowBlur = 15;
            },

            afterDraw: (chart) => {
                chart.ctx.shadowBlur = 0;
            }
        }]
    });
};