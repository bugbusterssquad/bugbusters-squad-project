import { test, expect } from "@playwright/test";

test.describe.serial("smoke", () => {
  const eventTitle = `E2E Event ${Date.now()}`;
  const startLocal = "2030-01-10T10:00";
  const endLocal = "2030-01-10T12:00";

  test("club admin creates event, publishes, submits SKS app and document", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("login-email").fill("admin1@bugbusters.dev");
    await page.getByTestId("login-password").fill("ClubAdmin123!");
    await page.getByTestId("login-submit").click();
    await expect(page.getByText("Profil")).toBeVisible();

    await page.goto("/admin");
    await page.getByTestId("admin-event-title").fill(eventTitle);
    await page.getByTestId("admin-event-start").fill(startLocal);
    await page.getByTestId("admin-event-end").fill(endLocal);
    await page.getByTestId("admin-event-capacity").fill("10");
    await page.getByTestId("admin-event-create").click();

    const eventCard = page.locator(`[data-event-title="${eventTitle}"]`);
    await expect(eventCard).toBeVisible();
    await eventCard.getByTestId("admin-event-publish").click();

    await page.getByTestId("admin-sks-submit").click();

    await page.getByTestId("admin-doc-event").selectOption({ label: eventTitle });
    await page.getByTestId("admin-doc-file").setInputFiles("e2e/fixtures/sample.pdf");
    await page.getByTestId("admin-doc-submit").click();
    await expect(page.getByText("Belge yüklendi.")).toBeVisible();

    await page.getByRole("button", { name: "Çıkış" }).first().click();
  });

  test("student registers for event and sees notification", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("login-email").fill("student@bugbusters.dev");
    await page.getByTestId("login-password").fill("Student123!");
    await page.getByTestId("login-submit").click();
    await expect(page.getByText("Profil")).toBeVisible();

    await page.goto("/events");
    await page.locator(`[data-event-title="${eventTitle}"]`).click();
    await page.getByTestId("event-register").click();
    await expect(page.getByText(/Durum: Registered|Durum: Waitlist/)).toBeVisible();

    await page.goto("/profile");
    await expect(page.getByTestId("notifications-list")).toContainText("event_registration");
  });

  test("sks reviews pending club application and document", async ({ page }) => {
    await page.goto("/login");
    await page.getByTestId("login-email").fill("sks@bugbusters.dev");
    await page.getByTestId("login-password").fill("SksAdmin123!");
    await page.getByTestId("login-submit").click();

    await page.goto("/sks");
    const approveClub = page.getByTestId("sks-approve-club").first();
    await expect(approveClub).toBeVisible();
    await approveClub.click();

    const approveDoc = page.getByTestId("sks-approve-document").first();
    await expect(approveDoc).toBeVisible();
    await approveDoc.click();
  });
});
