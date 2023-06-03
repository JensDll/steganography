resource "azurerm_service_plan" "plan" {
  name                = "steg_service_plan"
  location            = azurerm_resource_group.resources.location
  resource_group_name = azurerm_resource_group.resources.name
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "app" {
  name                = "steg-app"
  resource_group_name = azurerm_resource_group.resources.name
  location            = azurerm_service_plan.plan.location
  service_plan_id     = azurerm_service_plan.plan.id

  site_config {}
}
