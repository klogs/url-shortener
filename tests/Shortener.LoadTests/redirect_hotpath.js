/**
 * k6 load test — redirect hot-path
 *
 * Usage:
 *   k6 run redirect_hotpath.js
 *   k6 run --env BASE_URL=http://localhost:5001 --env SHORT_CODE=abc1234 redirect_hotpath.js
 *
 * Targets:
 *   - p95 < 50ms
 *   - error rate < 1%
 *   - 1000 VUs sustained for 30s
 */

import http from "k6/http";
import { check, sleep } from "k6";
import { Rate, Trend } from "k6/metrics";

const baseUrl = __ENV.BASE_URL || "http://localhost:5001";
const shortCode = __ENV.SHORT_CODE || "testcode";

const errorRate = new Rate("error_rate");
const redirectLatency = new Trend("redirect_latency", true);

export const options = {
  stages: [
    { duration: "10s", target: 200 },   // ramp up
    { duration: "30s", target: 1000 },  // sustained load
    { duration: "10s", target: 0 },     // ramp down
  ],
  thresholds: {
    http_req_duration: ["p(95)<50"],
    error_rate: ["rate<0.01"],
    redirect_latency: ["p(95)<50"],
  },
};

export default function () {
  const res = http.get(`${baseUrl}/${shortCode}`, {
    redirects: 0,
  });

  const ok = res.status === 302 || res.status === 301 || res.status === 307 || res.status === 308;
  check(res, {
    "redirect status (3xx)": () => ok,
    "has Location header": () => !!res.headers["Location"],
  });

  errorRate.add(!ok);
  redirectLatency.add(res.timings.duration);

  sleep(0.001); // 1ms think time
}

export function handleSummary(data) {
  return {
    stdout: JSON.stringify(
      {
        p95_ms: data.metrics.redirect_latency?.values?.["p(95)"],
        p99_ms: data.metrics.redirect_latency?.values?.["p(99)"],
        error_rate: data.metrics.error_rate?.values?.rate,
        rps: data.metrics.http_reqs?.values?.rate,
      },
      null,
      2
    ),
  };
}
