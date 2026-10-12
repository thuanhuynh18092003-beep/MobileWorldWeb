import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { createPhone } from './phone-model.js';
import { createIPhone18ProMax } from './iphone18-model.js';

const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
const backgroundCanvas = document.querySelector('#space-bg');
const heroCanvas = document.querySelector('#hero-device-canvas');
const dialog = document.querySelector('#device-viewer');
const viewerCanvas = document.querySelector('#device-viewer-canvas');
const pointer = { x: 0, y: 0, energy: 0 };
let warp = 0;
let warpTarget = 0;
let previousScroll = window.scrollY;
let previousFrame = 0;
let lastTrigger = null;

function rendererFor(canvas) {
    const renderer = new THREE.WebGLRenderer({
        canvas,
        alpha: true,
        antialias: window.innerWidth > 760,
        powerPreference: 'low-power'
    });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5));
    renderer.setClearColor(0x000000, 0);
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.55;
    return renderer;
}

function resize(scene) {
    const width = Math.max(1, scene.canvas.clientWidth);
    const height = Math.max(1, scene.canvas.clientHeight);
    scene.camera.aspect = width / height;
    scene.camera.updateProjectionMatrix();
    scene.renderer.setSize(width, height, false);
    scene.lastWidth = width;
    scene.lastHeight = height;
}

function addLighting(scene) {
    scene.add(new THREE.AmbientLight(0xbcaebd, 3.2));
    const burgundy = new THREE.PointLight(0xa31d3a, 105);
    burgundy.position.set(-3, 3, 4);
    const violet = new THREE.PointLight(0x7b2cbf, 78);
    violet.position.set(3, -1, 3);
    const blue = new THREE.PointLight(0x3a86ff, 28);
    blue.position.set(2, 3, -3);
    const frontFill = new THREE.PointLight(0xffc1d0, 24);
    frontFill.position.set(0, 1, 6);
    const backFill = new THREE.PointLight(0xd6789c, 38);
    backFill.position.set(-2, 0, -5);
    scene.add(burgundy, violet, blue, frontFill, backFill);
}

function nebulaTexture([red, green, blue]) {
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 512;
    const context = canvas.getContext('2d');
    for (let index = 0; index < 32; index++) {
        const x = 100 + Math.random() * 312;
        const y = 90 + Math.random() * 332;
        const radius = 65 + Math.random() * 145;
        const gradient = context.createRadialGradient(x, y, 0, x, y, radius);
        gradient.addColorStop(0, `rgba(${red},${green},${blue},${.045 + Math.random() * .055})`);
        gradient.addColorStop(1, `rgba(${red},${green},${blue},0)`);
        context.fillStyle = gradient;
        context.fillRect(0, 0, 512, 512);
    }
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    return texture;
}

function addNebula(scene) {
    const group = new THREE.Group();
    const layers = [
        { color: [163, 29, 58], x: -13, y: 4, z: -38, width: 64, height: 36 },
        { color: [123, 44, 191], x: 16, y: -6, z: -49, width: 72, height: 38 },
        { color: [58, 134, 255], x: 2, y: 12, z: -60, width: 58, height: 28 }
    ];
    for (const layer of layers) {
        const material = new THREE.SpriteMaterial({ map: nebulaTexture(layer.color), transparent: true, opacity: .82, depthWrite: false, blending: THREE.AdditiveBlending });
        const cloud = new THREE.Sprite(material);
        cloud.position.set(layer.x, layer.y, layer.z);
        cloud.scale.set(layer.width, layer.height, 1);
        cloud.userData.originX = layer.x;
        cloud.userData.originY = layer.y;
        group.add(cloud);
    }
    scene.add(group);
    return group;
}

function initBackground() {
    if (!backgroundCanvas) return null;
    const renderer = rendererFor(backgroundCanvas);
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(72, 1, .1, 120);
    camera.position.z = 18;
    const nebula = addNebula(scene);
    const count = window.innerWidth <= 760 ? 3200 : 6500;
    const positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);
    const palette = [new THREE.Color('#e0e0e6'), new THREE.Color('#a31d3a'), new THREE.Color('#b785d6'), new THREE.Color('#3a86ff')];
    for (let i = 0; i < count; i++) {
        const index = i * 3;
        positions[index] = (Math.random() - .5) * 75;
        positions[index + 1] = (Math.random() - .5) * 55;
        positions[index + 2] = (Math.random() - .5) * 70 - 10;
        const color = palette[Math.floor(Math.random() * palette.length)];
        colors[index] = color.r;
        colors[index + 1] = color.g;
        colors[index + 2] = color.b;
    }
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));
    const material = new THREE.PointsMaterial({
        size: .14,
        sizeAttenuation: true,
        vertexColors: true,
        transparent: true,
        opacity: .85,
        depthWrite: false,
        blending: THREE.AdditiveBlending
    });
    const stars = new THREE.Points(geometry, material);
    scene.add(stars);

    const streakCount = 550;
    const streakPositions = new Float32Array(streakCount * 6);
    const streakGeometry = new THREE.BufferGeometry();
    streakGeometry.setAttribute('position', new THREE.BufferAttribute(streakPositions, 3).setUsage(THREE.DynamicDrawUsage));
    const streakMaterial = new THREE.LineBasicMaterial({ color: 0xa31d3a, transparent: true, opacity: 0, depthWrite: false, blending: THREE.AdditiveBlending });
    const streaks = new THREE.LineSegments(streakGeometry, streakMaterial);
    scene.add(streaks);

    const background = { canvas: backgroundCanvas, renderer, scene, camera, nebula, stars, streaks, positions, streakPositions, streakMaterial, streakCount };
    resize(background);
    renderer.render(scene, camera);
    backgroundCanvas.dataset.ready = 'true';
    backgroundCanvas.dataset.starCount = String(count);
    return background;
}

function updateStreaks(background) {
    background.streakMaterial.opacity = Math.min(.58, warp * .56);
    if (warp < .025) return;
    for (let i = 0; i < background.streakCount; i++) {
        const source = i * 3;
        const target = i * 6;
        const x = background.positions[source];
        const y = background.positions[source + 1];
        const z = background.positions[source + 2];
        background.streakPositions[target] = x;
        background.streakPositions[target + 1] = y;
        background.streakPositions[target + 2] = z;
        background.streakPositions[target + 3] = x;
        background.streakPositions[target + 4] = y;
        background.streakPositions[target + 5] = z + warp * 4.5;
    }
    background.streaks.geometry.attributes.position.needsUpdate = true;
}

function initHero() {
    if (!heroCanvas) return null;
    const renderer = rendererFor(heroCanvas);
    renderer.toneMappingExposure = 1.05;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(38, 1, .1, 50);
    camera.position.set(0, 0, 7.7);
    scene.add(new THREE.AmbientLight(0xd7b8c7, 2.1));
    const key = new THREE.PointLight(0xffbacb, 24);
    key.position.set(-3.2, 3, 4);
    const fill = new THREE.PointLight(0x9752a8, 18);
    fill.position.set(3, -1, 2);
    const rim = new THREE.PointLight(0x6d87ef, 15);
    rim.position.set(1.5, 1.8, -3);
    const rear = new THREE.PointLight(0xdb8099, 34);
    rear.position.set(-3, 2, -4);
    scene.add(key, fill, rim, rear);
    const phone = createIPhone18ProMax();
    phone.group.rotation.set(.05, -.38, -.08);
    scene.add(phone.group);
    const hero = { canvas: heroCanvas, renderer, scene, camera, phone, angle: -.38 };
    resize(hero);
    renderer.render(scene, camera);
    heroCanvas.parentElement.classList.add('is-3d');
    heroCanvas.dataset.ready = 'true';
    return hero;
}

let background = null;
let hero = null;
let viewer = null;
try { background = initBackground(); } catch (error) { console.warn('Không thể khởi tạo nền WebGL:', error); if (backgroundCanvas) backgroundCanvas.hidden = true; }
try { hero = initHero(); } catch (error) { console.warn('Không thể khởi tạo mô hình iPhone 18 Pro Max:', error); if (heroCanvas) heroCanvas.hidden = true; }

if (background || hero) {
    document.body.classList.add('has-3d');
    document.querySelectorAll('.details-3d-button').forEach(button => { button.hidden = false; });
}

function initViewer() {
    if (viewer) return viewer;
    const renderer = rendererFor(viewerCanvas);
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(38, 1, .1, 50);
    camera.position.set(0, 0, 7.5);
    addLighting(scene);
    const phone = createPhone();
    phone.group.rotation.y = .2;
    scene.add(phone.group);
    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = .08;
    controls.enablePan = false;
    controls.minDistance = 4.3;
    controls.maxDistance = 10;
    controls.autoRotate = !reducedMotion.matches;
    controls.autoRotateSpeed = .7;
    viewer = { canvas: viewerCanvas, renderer, scene, camera, phone, controls };
    viewerCanvas.dataset.ready = 'true';
    return viewer;
}

function openViewer(trigger) {
    if (!dialog || !viewerCanvas) return false;
    try {
        const productName = trigger.dataset.productName || 'Điện thoại';
        const productColor = trigger.dataset.productColor || '#800020';
        const detailUrl = trigger.dataset.productUrl || trigger.getAttribute('href') || window.location.pathname;
        const model = initViewer();
        model.phone.setColor(productColor);
        model.controls.reset();
        dialog.querySelector('#device-viewer-title').textContent = `${productName} · mô phỏng 3D`;
        dialog.querySelector('#device-viewer-detail').setAttribute('href', detailUrl);
        lastTrigger = trigger;
        dialog.showModal();
        document.body.classList.add('viewer-open');
        resize(model);
        model.renderer.render(model.scene, model.camera);
        return true;
    } catch (error) {
        console.warn('Không thể mở mô phỏng 3D:', error);
        if (dialog.open) dialog.close();
        document.body.classList.remove('has-3d', 'viewer-open');
        document.querySelectorAll('.details-3d-button').forEach(button => { button.hidden = true; });
        return false;
    }
}

document.addEventListener('click', event => {
    const trigger = event.target.closest('[data-view-3d]');
    if (!trigger || !document.body.classList.contains('has-3d')) return;
    event.preventDefault();
    if (!openViewer(trigger) && trigger.matches('a[href]')) window.location.assign(trigger.href);
});
dialog?.querySelector('.device-viewer-close')?.addEventListener('click', () => dialog.close());
dialog?.addEventListener('click', event => { if (event.target === dialog) dialog.close(); });
dialog?.addEventListener('close', () => { document.body.classList.remove('viewer-open'); lastTrigger?.focus(); });

document.addEventListener('pointermove', event => {
    pointer.x = (event.clientX / window.innerWidth - .5) * 2;
    pointer.y = (event.clientY / window.innerHeight - .5) * 2;
    pointer.energy = Math.min(1, pointer.energy + Math.hypot(event.movementX, event.movementY) / 230);
}, { passive: true });
window.addEventListener('scroll', () => {
    const distance = Math.abs(window.scrollY - previousScroll);
    previousScroll = window.scrollY;
    warpTarget = Math.max(warpTarget, Math.min(1, distance / 280));
}, { passive: true });
window.addEventListener('resize', () => {
    if (background) resize(background);
    if (hero) resize(hero);
    if (viewer && dialog.open) resize(viewer);
}, { passive: true });

function animate(time) {
    requestAnimationFrame(animate);
    if (document.hidden || time - previousFrame < 1000 / 45) return;
    const delta = Math.min((time - previousFrame) / 1000 || 0, .05);
    previousFrame = time;
    if (background) {
        if (!reducedMotion.matches) {
            warp += (warpTarget - warp) * .14;
            warpTarget *= .89;
            background.stars.rotation.y += delta * (.012 + warp * .38);
            background.stars.rotation.x += (pointer.y * .025 - background.stars.rotation.x) * .015;
            background.stars.rotation.z += (pointer.x * .02 + pointer.energy * .01 - background.stars.rotation.z) * .02;
            background.stars.material.size = .14 + pointer.energy * .04 + warp * .06;
            pointer.energy *= .94;
            background.camera.position.x += (pointer.x * .55 - background.camera.position.x) * .025;
            background.camera.position.z += (18 - warp * 3.8 - background.camera.position.z) * .08;
            background.nebula.rotation.z += delta * .0018;
            background.nebula.children.forEach((cloud, index) => {
                cloud.position.x += (cloud.userData.originX + pointer.x * (.45 + index * .22) - cloud.position.x) * .012;
                cloud.position.y += (cloud.userData.originY - Math.min(3, window.scrollY * .0006) * (.25 + index * .17) - cloud.position.y) * .012;
            });
            updateStreaks(background);
        }
        background.renderer.render(background.scene, background.camera);
    }
    if (hero) {
        const bounds = hero.canvas.getBoundingClientRect();
        if (bounds.width > 0 && bounds.height > 0 && bounds.bottom > 0 && bounds.top < window.innerHeight) {
            if (hero.lastWidth !== hero.canvas.clientWidth || hero.lastHeight !== hero.canvas.clientHeight) resize(hero);
            if (!reducedMotion.matches) {
                hero.angle += delta * .31;
                hero.phone.group.rotation.y = hero.angle + pointer.x * .1;
                hero.phone.group.rotation.x += (.05 - pointer.y * .12 - hero.phone.group.rotation.x) * .05;
            }
            hero.renderer.render(hero.scene, hero.camera);
        }
    }
    if (viewer && dialog.open) {
        viewer.controls.update(delta);
        viewer.renderer.render(viewer.scene, viewer.camera);
    }
}
if (background || hero) requestAnimationFrame(animate);
