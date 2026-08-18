import { onBeforeUnmount, ref, watch, type Ref } from 'vue';
import gsap from 'gsap';

/**
 * 高性能折叠动画：仅动画 height + opacity（合成层友好，遵循 gsap-performance）
 */
export function useCollapseAnimation(expanded: Ref<boolean>) {
  const contentRef = ref<HTMLElement | null>(null);
  let tween: gsap.core.Tween | null = null;

  function killTween() {
    tween?.kill();
    tween = null;
  }

  function animate(show: boolean) {
    const el = contentRef.value;
    if (!el) return;

    killTween();

    if (show) {
      gsap.set(el, { display: 'block', overflow: 'hidden' });
      const targetHeight = el.scrollHeight;
      gsap.set(el, { height: 0, opacity: 0 });
      tween = gsap.to(el, {
        height: targetHeight,
        opacity: 1,
        duration: 0.32,
        ease: 'power2.out',
        onComplete: () => {
          gsap.set(el, { height: 'auto', overflow: 'visible' });
        }
      });
    } else {
      gsap.set(el, { overflow: 'hidden' });
      const currentHeight = el.offsetHeight;
      gsap.set(el, { height: currentHeight });
      tween = gsap.to(el, {
        height: 0,
        opacity: 0,
        duration: 0.26,
        ease: 'power2.in',
        onComplete: () => {
          gsap.set(el, { display: 'none' });
        }
      });
    }
  }

  watch(expanded, val => animate(val), { flush: 'post' });

  onBeforeUnmount(killTween);

  return { contentRef, animate };
}
