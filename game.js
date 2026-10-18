const c=document.getElementById('game'),ctx=c.getContext('2d');
const moneyEl=document.getElementById('money'),cargoEl=document.getElementById('cargo'),hintEl=document.getElementById('hint');
let W=0,H=0,dpr=1; function resize(){dpr=Math.min(devicePixelRatio||1,2);W=innerWidth;H=innerHeight;c.width=W*dpr;c.height=H*dpr;ctx.setTransform(dpr,0,0,dpr,0,0)} addEventListener('resize',resize);resize();
const world={w:1800,h:1300,money:0,logs:0,planks:0,process:0};
const p={x:600,y:650,r:18,speed:230,face:0};
const mill={x:980,y:570,w:170,h:120}; const sell={x:1260,y:560,w:170,h:145};
const trees=[]; function addTree(x,y){trees.push({x,y,r:30,hp:5,max:5,dead:false,respawn:0,hit:0})}
[[300,310],[420,270],[530,350],[280,500],[430,520],[310,720],[480,790],[610,260],[690,390],[270,930],[510,1000],[710,880]].forEach(a=>addTree(...a));
const keys={};addEventListener('keydown',e=>keys[e.key.toLowerCase()]=1);addEventListener('keyup',e=>keys[e.key.toLowerCase()]=0);
let joy={x:0,y:0,id:null};const jb=document.getElementById('joystick'),stick=document.getElementById('stick');
function joyMove(e){const t=[...e.changedTouches].find(t=>t.identifier===joy.id);if(!t)return;const r=jb.getBoundingClientRect(),cx=r.left+r.width/2,cy=r.top+r.height/2;let dx=t.clientX-cx,dy=t.clientY-cy;const m=Math.hypot(dx,dy),lim=42;if(m>lim){dx*=lim/m;dy*=lim/m}joy.x=dx/lim;joy.y=dy/lim;stick.style.transform=`translate(${dx}px,${dy}px)`}
jb.addEventListener('touchstart',e=>{e.preventDefault();joy.id=e.changedTouches[0].identifier;joyMove(e)},{passive:false});jb.addEventListener('touchmove',e=>{e.preventDefault();joyMove(e)},{passive:false});
function joyEnd(e){if([...e.changedTouches].some(t=>t.identifier===joy.id)){joy={x:0,y:0,id:null};stick.style.transform='translate(0,0)'}}jb.addEventListener('touchend',joyEnd);jb.addEventListener('touchcancel',joyEnd);
function clamp(v,a,b){return Math.max(a,Math.min(b,v))} function nearRect(o,r,pad=50){return o.x>r.x-pad&&o.x<r.x+r.w+pad&&o.y>r.y-pad&&o.y<r.y+r.h+pad}
let chopTimer=0,processLock=0,sellLock=0;
function update(dt){let x=joy.x,y=joy.y;if(keys.w||keys.arrowup)y-=1;if(keys.s||keys.arrowdown)y+=1;if(keys.a||keys.arrowleft)x-=1;if(keys.d||keys.arrowright)x+=1;let m=Math.hypot(x,y);if(m>1){x/=m;y/=m}if(m>.05){p.x=clamp(p.x+x*p.speed*dt,40,world.w-40);p.y=clamp(p.y+y*p.speed*dt,40,world.h-40);p.face=Math.atan2(y,x)}
let target=null,bd=78;for(const t of trees){if(t.dead){t.respawn-=dt;if(t.respawn<=0){t.dead=false;t.hp=t.max}}else{const d=Math.hypot(p.x-t.x,p.y-t.y);if(d<bd){bd=d;target=t}}t.hit=Math.max(0,t.hit-dt)}
if(target&&world.logs<3){chopTimer+=dt;hintEl.textContent=`Chopping tree ${target.hp}/${target.max}`;if(chopTimer>.55){chopTimer=0;target.hp--;target.hit=.16;if(target.hp<=0){target.dead=true;target.respawn=12;world.logs=Math.min(3,world.logs+2)}}}else chopTimer=0;
processLock-=dt;sellLock-=dt;if(nearRect(p,mill,45)){if(world.logs>0&&processLock<=0){world.logs--;world.process++;processLock=.45}hintEl.textContent=world.logs?'Feeding logs to sawmill':'Sawmill'}
if(world.process>0){world.process=Math.max(0,world.process-dt*.6);if(world.process===0)world.planks+=4}
if(nearRect(p,sell,45)){if(world.planks>0&&sellLock<=0){world.planks--;world.money+=8;sellLock=.18}hintEl.textContent=world.planks?'Selling planks':'Sell zone'}
if(!target&&!nearRect(p,mill,45)&&!nearRect(p,sell,45))hintEl.textContent=world.logs?'Take logs to the sawmill':world.planks?'Take planks to SELL':'Walk to a tree';
moneyEl.textContent='$'+world.money;cargoEl.textContent=world.logs?`Logs ${world.logs}/3`:world.planks?`Planks ${world.planks}`:'Empty';}
function iso(x,y,cam){return{x:(x-y)*.62+W/2-cam.x,y:(x+y)*.31+H/2-cam.y}}
function poly(points,fill,stroke){ctx.beginPath();points.forEach((q,i)=>(i?ctx.lineTo(q.x,q.y):ctx.moveTo(q.x,q.y)));ctx.closePath();ctx.fillStyle=fill;ctx.fill();if(stroke){ctx.strokeStyle=stroke;ctx.lineWidth=2;ctx.stroke()}}
function draw(){ctx.clearRect(0,0,W,H);ctx.fillStyle='#9dd07a';ctx.fillRect(0,0,W,H);const pi=iso(p.x,p.y,{x:0,y:0});const cam={x:pi.x-W/2,y:pi.y-H*.55};
let a=iso(0,0,cam),b=iso(world.w,0,cam),d=iso(world.w,world.h,cam),e=iso(0,world.h,cam);poly([a,b,d,e],'#8fc56d','#6da44f');
function pad(r,color,label){let q=[iso(r.x,r.y,cam),iso(r.x+r.w,r.y,cam),iso(r.x+r.w,r.y+r.h,cam),iso(r.x,r.y+r.h,cam)];poly(q,color,'#ffffff55');let z=iso(r.x+r.w/2,r.y+r.h/2,cam);ctx.fillStyle='#fff';ctx.font='900 15px system-ui';ctx.textAlign='center';ctx.fillText(label,z.x,z.y+4)}pad(mill,'#a87343','SAWMILL');pad(sell,'#d6a52d','SELL $8');
const objs=[];trees.forEach(t=>{if(!t.dead)objs.push({y:t.x+t.y,type:'tree',o:t})});objs.push({y:p.x+p.y,type:'player',o:p});objs.sort((u,v)=>u.y-v.y);for(const it of objs){if(it.type==='tree'){const t=it.o,z=iso(t.x,t.y,cam);ctx.fillStyle='#704427';ctx.fillRect(z.x-6,z.y-35,12,38);ctx.beginPath();ctx.arc(z.x,z.y-49,t.r+(t.hit?5:0),0,Math.PI*2);ctx.fillStyle=t.hit?'#b5dd68':'#397c42';ctx.fill();if(t.hp<t.max){ctx.fillStyle='#2229';ctx.fillRect(z.x-24,z.y-88,48,6);ctx.fillStyle='#fff';ctx.fillRect(z.x-24,z.y-88,48*t.hp/t.max,6)}}else{const z=iso(p.x,p.y,cam);ctx.beginPath();ctx.ellipse(z.x,z.y+7,18,9,0,0,Math.PI*2);ctx.fillStyle='#0003';ctx.fill();ctx.beginPath();ctx.arc(z.x,z.y-17,14,0,Math.PI*2);ctx.fillStyle='#f0b47e';ctx.fill();ctx.fillStyle='#2f5f9c';ctx.fillRect(z.x-13,z.y-5,26,30);ctx.strokeStyle='#5b351e';ctx.lineWidth=5;ctx.beginPath();ctx.moveTo(z.x+8,z.y);ctx.lineTo(z.x+25,z.y-18);ctx.stroke()}}
let mz=iso(mill.x+mill.w/2,mill.y+mill.h/2,cam);ctx.fillStyle='#513d31';ctx.fillRect(mz.x-45,mz.y-45,90,45);ctx.fillStyle='#ddd';ctx.beginPath();ctx.arc(mz.x,mz.y-24,28,0,Math.PI*2);ctx.fill();ctx.strokeStyle='#777';ctx.lineWidth=4;ctx.stroke();
requestAnimationFrame(loop)}let last=performance.now();function loop(now){const dt=Math.min((now-last)/1000,.04);last=now;update(dt);draw()}requestAnimationFrame(loop);
