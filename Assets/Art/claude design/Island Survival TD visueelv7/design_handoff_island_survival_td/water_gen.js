// Water tile generator — voronoi cell network, seamless on a torus, 12 baked frames.
// Same style rules as the shipped 64x64 tile; adds variable cell density and larger canvases.

function makeRng(a) {
  return function () {
    a |= 0; a = a + 0x6D2B79F5 | 0;
    let t = Math.imul(a ^ a >>> 15, 1 | a);
    t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
    return ((t ^ t >>> 14) >>> 0) / 4294967296;
  };
}

const PAL = {
  BASE: [69, 109, 245],   // #456DF5
  DARK: [63, 101, 239],   // #3F65EF
  T1:   [86, 125, 243],   // #567DF3
  T2:   [106, 143, 238],  // #6A8FEE
  EDGE: [128, 162, 232],  // #80A2E8
  GLINT:[170, 195, 241],  // #AAC3F1
  FOAM: [255, 255, 255]
};

function wrapd(a, b, m) { const d = Math.abs(a - b); return d > m / 2 ? m - d : d; }
function wrapDelta(a, b, m) { let d = a - b; if (d > m / 2) d -= m; if (d < -m / 2) d += m; return d; }

// Seamless low-frequency field in [-1,1] — drives regional cell-density variation.
function densityField(x, y, W, H) {
  const u = x / W * Math.PI * 2, v = y / H * Math.PI * 2;
  return (Math.sin(u) * 0.45 + Math.sin(v * 2 + 1.1) * 0.3 +
          Math.sin(u * 2 + v + 2.4) * 0.35 + Math.sin(u - v * 3 + 0.7) * 0.2) / 1.3;
}

// Bucket grid over the torus for nearest-seed queries.
function makeBuckets(pts, W, H, cell) {
  const gw = Math.max(1, Math.round(W / cell)), gh = Math.max(1, Math.round(H / cell));
  const cw = W / gw, ch = H / gh;
  const buckets = new Array(gw * gh);
  for (let i = 0; i < buckets.length; i++) buckets[i] = [];
  for (let i = 0; i < pts.length; i++) {
    const gx = Math.min(gw - 1, Math.floor(pts[i].x / cw));
    const gy = Math.min(gh - 1, Math.floor(pts[i].y / ch));
    buckets[gy * gw + gx].push(i);
  }
  return { buckets, gw, gh, cw, ch };
}

function neighbourhood(bg, x, y, ring) {
  const gx = Math.min(bg.gw - 1, Math.floor(x / bg.cw));
  const gy = Math.min(bg.gh - 1, Math.floor(y / bg.ch));
  const out = [];
  for (let dy = -ring; dy <= ring; dy++) {
    const yy = ((gy + dy) % bg.gh + bg.gh) % bg.gh;
    for (let dx = -ring; dx <= ring; dx++) {
      const xx = ((gx + dx) % bg.gw + bg.gw) % bg.gw;
      const b = bg.buckets[yy * bg.gw + xx];
      for (let k = 0; k < b.length; k++) out.push(b[k]);
    }
  }
  return out;
}

// Per-bucket candidate lists, computed once per bucket instead of once per pixel.
// This is what makes the 256px canvases tractable.
function bucketNeighbourLists(bg, ring) {
  const lists = new Array(bg.gw * bg.gh);
  for (let gy = 0; gy < bg.gh; gy++) {
    for (let gx = 0; gx < bg.gw; gx++) {
      const out = [];
      for (let dy = -ring; dy <= ring; dy++) {
        const yy = ((gy + dy) % bg.gh + bg.gh) % bg.gh;
        for (let dx = -ring; dx <= ring; dx++) {
          const xx = ((gx + dx) % bg.gw + bg.gw) % bg.gw;
          const b = bg.buckets[yy * bg.gw + xx];
          for (let k = 0; k < b.length; k++) out.push(b[k]);
        }
      }
      lists[gy * bg.gw + gx] = Int32Array.from(out);
    }
  }
  return lists;
}

// Dart-throwing blue-noise sampling with a locally varying minimum radius.
// Allocation-free inner loop — at 1024² this runs ~500k trials.
function sampleSeeds(W, H, rBase, rnd, tries) {
  const rMin = rBase * 0.72, rMax = rBase * 1.32;
  const PX = [], PY = [];
  const cell = rMax * 1.05;
  const gw = Math.max(1, Math.round(W / cell)), gh = Math.max(1, Math.round(H / cell));
  const cw = W / gw, ch = H / gh;
  const grid = new Array(gw * gh);
  for (let i = 0; i < grid.length; i++) grid[i] = [];
  const HW = W / 2, HH = H / 2;
  for (let t = 0; t < tries; t++) {
    const x = rnd() * W, y = rnd() * H;
    const f = densityField(x, y, W, H);
    const r = rBase * (1 + f * 0.30);
    const r2 = r * r;
    const gx = Math.min(gw - 1, Math.floor(x / cw));
    const gy = Math.min(gh - 1, Math.floor(y / ch));
    let ok = true;
    for (let dy = -2; dy <= 2 && ok; dy++) {
      const yy = ((gy + dy) % gh + gh) % gh;
      for (let dx = -2; dx <= 2 && ok; dx++) {
        const xx = ((gx + dx) % gw + gw) % gw;
        const b = grid[yy * gw + xx];
        for (let k = 0; k < b.length; k++) {
          const i = b[k];
          let ddx = x - PX[i]; if (ddx < 0) ddx = -ddx; if (ddx > HW) ddx = W - ddx;
          let ddy = y - PY[i]; if (ddy < 0) ddy = -ddy; if (ddy > HH) ddy = H - ddy;
          if (ddx * ddx + ddy * ddy < r2) { ok = false; break; }
        }
      }
    }
    if (!ok) continue;
    grid[gy * gw + gx].push(PX.length);
    PX.push(x); PY.push(y);
  }
  const pts = [];
  for (let i = 0; i < PX.length; i++) pts.push({ x: PX[i], y: PY[i] });
  return { pts, rMin, rMax };
}

// Lloyd relaxation on the torus — rounds the cells off without fully flattening
// the regional density variation (few passes on purpose).
// `stride` subsamples the integration grid; 2 is visually identical to 1 and 4x faster.
function relax(pts, W, H, passes, bucketCell, ring, stride) {
  ring = ring || 2; stride = stride || 1;
  for (let it = 0; it < passes; it++) {
    const acc = pts.map(p => ({ sx: 0, sy: 0, n: 0, ox: p.x, oy: p.y }));
    const bg = makeBuckets(pts, W, H, bucketCell);
    const lists = bucketNeighbourLists(bg, ring);
    const PX = new Float64Array(pts.length), PY = new Float64Array(pts.length);
    for (let i = 0; i < pts.length; i++) { PX[i] = pts[i].x; PY[i] = pts[i].y; }
    const HW = W / 2, HH = H / 2;
    for (let y = 0; y < H; y += stride) {
      const fy = y + 0.5;
      const gy = Math.min(bg.gh - 1, Math.floor(fy / bg.ch));
      for (let x = 0; x < W; x += stride) {
        const fx = x + 0.5;
        const cand = lists[gy * bg.gw + Math.min(bg.gw - 1, Math.floor(fx / bg.cw))];
        let best = Infinity, bi = -1;
        for (let i = 0, L = cand.length; i < L; i++) {
          const ci = cand[i];
          let dx = fx - PX[ci]; if (dx < 0) dx = -dx; if (dx > HW) dx = W - dx;
          let dy = fy - PY[ci]; if (dy < 0) dy = -dy; if (dy > HH) dy = H - dy;
          const d = dx * dx + dy * dy;
          if (d < best) { best = d; bi = ci; }
        }
        if (bi < 0) continue;
        const a = acc[bi];
        a.sx += wrapDelta(fx, a.ox, W); a.sy += wrapDelta(fy, a.oy, H); a.n++;
      }
    }
    pts = acc.map((a, i) => a.n === 0 ? pts[i] : {
      x: ((a.ox + a.sx / a.n) % W + W) % W,
      y: ((a.oy + a.sy / a.n) % H + H) % H
    });
  }
  return pts;
}

// Renders frames [from, to) of the cycle. Returns {canvases, datas}.
function renderFrames(seeds, W, H, frames, rnd, bucketCell, createCanvas, from, to, ring) {
  from = from === undefined ? 0 : from;
  to = to === undefined ? frames : to;
  ring = ring || 2;
  const specks = [];
  const speckCount = Math.round(22 * (W * H) / 4096);
  for (let i = 0; i < speckCount; i++) {
    specks.push({
      x: Math.floor(rnd() * W), y: Math.floor(rnd() * H),
      on: Math.floor(rnd() * frames), len: 3 + Math.floor(rnd() * 3),
      shape: Math.floor(rnd() * 3)
    });
  }
  const canvases = [], datas = [];
  const HW = W / 2, HH = H / 2;
  const SX = new Float64Array(seeds.length), SY = new Float64Array(seeds.length);
  const PH = new Float64Array(seeds.length), AM = new Float64Array(seeds.length);
  for (let i = 0; i < seeds.length; i++) {
    SX[i] = seeds[i].x; SY[i] = seeds[i].y; PH[i] = seeds[i].ph; AM[i] = seeds[i].amp;
  }
  for (let f = from; f < to; f++) {
    const c = createCanvas(W, H), ctx = c.getContext('2d');
    const im = ctx.createImageData(W, H), D = im.data;
    const t = f / frames * Math.PI * 2;
    const pts = [];
    const PX = new Float64Array(seeds.length), PY = new Float64Array(seeds.length);
    for (let i = 0; i < seeds.length; i++) {
      const px = (SX[i] + Math.cos(PH[i] + t) * AM[i] + W) % W;
      const py = (SY[i] + Math.sin(PH[i] + t) * AM[i] + H) % H;
      PX[i] = px; PY[i] = py;
      pts.push({ x: px, y: py, id: i });
    }
    const bg = makeBuckets(pts, W, H, bucketCell);
    const lists = bucketNeighbourLists(bg, ring);
    for (let y = 0; y < H; y++) {
      const fy = y + 0.5;
      const gy = Math.min(bg.gh - 1, Math.floor(fy / bg.ch));
      for (let x = 0; x < W; x++) {
        const fx = x + 0.5;
        const cand = lists[gy * bg.gw + Math.min(bg.gw - 1, Math.floor(fx / bg.cw))];
        let d1 = Infinity, d2 = Infinity, i1 = -1, i2 = -1;
        for (let k = 0, L = cand.length; k < L; k++) {
          const ci = cand[k];
          let dx = fx - PX[ci]; if (dx < 0) dx = -dx; if (dx > HW) dx = W - dx;
          let dy = fy - PY[ci]; if (dy < 0) dy = -dy; if (dy > HH) dy = H - dy;
          const d = dx * dx + dy * dy;
          if (d < d1) { d2 = d1; i2 = i1; d1 = d; i1 = ci; }
          else if (d < d2) { d2 = d; i2 = ci; }
        }
        const gap = Math.sqrt(d2) - Math.sqrt(d1);
        const h = ((i1 * 73856093) ^ (i2 * 19349663)) >>> 0;
        const glint = ((h >> 17) % 100 < 6) && ((((h ^ (f * 2654435761)) >>> 0) % 100) < 40);
        let col;
        if (gap < 0.55)      col = ((h % 100) < 14 || glint) ? PAL.GLINT : PAL.EDGE;
        else if (gap < 1.15) col = PAL.T2;
        else if (gap < 1.9)  col = PAL.T1;
        else col = ((i1 * 2246822519) >>> 13) % 100 < 10 ? PAL.DARK : PAL.BASE;
        if (gap < 1.9 && (h >> 7) % 100 < 7) col = PAL.BASE; // occasional break in the mesh
        const o = (y * W + x) * 4;
        D[o] = col[0]; D[o + 1] = col[1]; D[o + 2] = col[2]; D[o + 3] = 255;
      }
    }
    for (const s of specks) {
      let vis = false;
      for (let k = 0; k < s.len; k++) if ((s.on + k) % frames === f) vis = true;
      if (!vis) continue;
      const put = (dx, dy) => {
        const px = (s.x + dx + W) % W, py = (s.y + dy + H) % H, o = (py * W + px) * 4;
        D[o] = 255; D[o + 1] = 255; D[o + 2] = 255; D[o + 3] = 255;
      };
      put(0, 0);
      if (s.shape === 1) put(1, 0);
      if (s.shape === 2) { put(0, 1); put(1, 1); }
    }
    ctx.putImageData(im, 0, 0);
    canvases.push(c); datas.push(D);
  }
  return { canvases, datas };
}

function worstFrameDelta(datas) {
  let worst = 0;
  const n = datas.length;
  for (let f = 0; f < n; f++) {
    const A = datas[f], B = datas[(f + 1) % n];
    let c = 0;
    for (let i = 0; i < A.length; i += 4)
      if (A[i] !== B[i] || A[i + 1] !== B[i + 1] || A[i + 2] !== B[i + 2]) c++;
    worst = Math.max(worst, c);
  }
  return worst / (datas[0].length / 4);
}

// Seeds only — sampling + relaxation, serialisable so rendering can run separately.
function buildSeeds({ W, H, seed, rBase, relaxPasses, ring, stride, triesFactor }) {
  const rnd = makeRng(seed);
  const tries = Math.round(W * H * (triesFactor || 0.9));
  const { pts, rMax } = sampleSeeds(W, H, rBase, rnd, tries);
  const bucketCell = rMax * (ring === 1 ? 1.55 : 1.05);
  const relaxed = relax(pts, W, H, relaxPasses, bucketCell, ring, stride);
  const seeds = relaxed.map((p, i) => ({
    x: p.x, y: p.y, ph: rnd() * Math.PI * 2, amp: 0.18 + rnd() * 0.22, id: i
  }));
  return { seeds, bucketCell, W, H };
}

function packSeeds(s) {
  const r3 = v => Math.round(v * 1000) / 1000;
  return JSON.stringify({
    W: s.W, H: s.H, bucketCell: s.bucketCell,
    x: s.seeds.map(v => r3(v.x)), y: s.seeds.map(v => r3(v.y)),
    ph: s.seeds.map(v => r3(v.ph)), amp: s.seeds.map(v => r3(v.amp))
  });
}

function unpackSeeds(json) {
  const o = JSON.parse(json);
  const seeds = o.x.map((_, i) => ({ x: o.x[i], y: o.y[i], ph: o.ph[i], amp: o.amp[i], id: i }));
  return { seeds, bucketCell: o.bucketCell, W: o.W, H: o.H };
}

function buildTile({ W, H, seed, rBase, frames, relaxPasses, createCanvas }) {
  const s = buildSeeds({ W, H, seed, rBase, relaxPasses });
  const out = renderFrames(s.seeds, W, H, frames, makeRng(seed ^ 0x5f5f), s.bucketCell, createCanvas);
  return { seedCount: s.seeds.length, worstDelta: worstFrameDelta(out.datas), ...out };
}
